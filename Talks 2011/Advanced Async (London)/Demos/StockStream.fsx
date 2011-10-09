#r "FSharp.AsyncExtensions.dll"

open System
open System.Net
open System.Text
open FSharp.IO
open FSharp.Control
open Microsoft.FSharp.Control.WebExtensions

// ----------------------------------------------------------------------------
// Load FSharpChart and define chart style

#load "FSharpChart.fsx"
open System.Drawing
open MSDN.FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting

// Defines the grid color (lines under the chart)
let withNiceStyle (min, max) chart =
  let dashGrid = 
      Grid( LineColor = Color.Gainsboro, 
            LineDashStyle = ChartDashStyle.Dash )
  chart
  |> FSharpChart.WithArea.AxisX(MajorGrid = dashGrid)
  |> FSharpChart.WithArea.AxisY(MajorGrid = dashGrid, Minimum = min, Maximum = max)


// ----------------------------------------------------------------------------
// Asynchronously download lines from the internet (using ASCII)

let downloadLines (url:string) = asyncSeq {
  // Create HTTP request and get response asynchronously
  let req = HttpWebRequest.Create(url)
  use! resp = req.AsyncGetResponse()
  use stream = resp.GetResponseStream()

  // Download content in 1kB buffers 
  let str = ref ""
  for buffer in stream.AsyncReadSeq(1024) do
    str := str.Value + String(Encoding.ASCII.GetChars(buffer)) + " "
    let parts = str.Value.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
    for i in 0 .. parts.Length - 2 do
      yield parts.[i]

    let rest = parts.[parts.Length - 1]
    str := rest.Substring(0, rest.Length - 1)

  // Yield the last line if it is not empty
  if str.Value <> "" then yield str.Value }


// ----------------------------------------------------------------------------
// Getting stock prices from Yahoo as asynchronous sequence

let ystock = "http://ichart.finance.yahoo.com/table.csv?s="

async {
  for line in downloadLines (ystock + "MSFT") do
    printfn "%s" line }
|> Async.Start


let parsed = 
  asyncSeq {
    for line in downloadLines (ystock + "MSFT") |> AsyncSeq.skip 1 do
      let infos = (line:string).Split(',')
      let ohlc = float infos.[1], float infos.[2], float infos.[3], float infos.[4]
      yield ohlc }


parsed
|> AsyncSeq.take 10
|> AsyncSeq.iter (printfn "%A")
|> Async.Start


downloadLines (ystock + "MSFT")
|> AsyncSeq.skip 1
|> AsyncSeq.mapAsync (fun line -> async {
    do! Async.Sleep(100)
    let infos = line.Split(',')
    return float infos.[1], float infos.[2], float infos.[3], float infos.[4] })
|> AsyncSeq.take 100
|> AsyncSeq.toObservable
|> FSharpChart.Candlestick
|> withNiceStyle (20.0, 30.0)
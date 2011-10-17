open System
open System.Drawing

#r "System.Windows.Forms.DataVisualization.dll"
#r "bin\\MSDN.FSharpChart.dll"
#load "bin\\FSharpChart.fsx"
open MSDN.FSharp.Charting

#load "1_Data.fsx"
open StockData


// ------------------------------------------------------------------
// Parallelizing data processing usin PLINQ
// ------------------------------------------------------------------

#load "bin\\pseq.fs"
open Microsoft.FSharp.Collections

let prices = loadStockPrices "aapl" |> Array.rev

// Calculate standard deviation and average value 
// over a sliding window of the specified size
let slidingWindow size =
  prices
  |> Seq.windowed size
  |> Seq.map (Array.map (fun (dt, (o, h, l, c)) -> dt, o * 1.0))
  |> Seq.map (fun win -> 
      let dt, _ = win.[size/2]
      let win = win |> Array.map snd
      let avg = win |> Array.average 
      let sum = win |> Array.sumBy (fun v -> (v - avg) * (v - avg)) 
      dt, avg, sqrt (sum / float win.Length) )
  |> Seq.toArray

// Get the opening price values
let values skip = 
  prices |> Array.map (fun (dt, (o,h,l,c)) -> dt, o)

// ==================================================================
// TASK #8
// The 'slidingWindow' operation may be quite slow if the sliding
// window is large. We can run the processing in parallel using 
// PLINQ (and the PSeq) module from F# PowerPack.
//
// To use this, change the functions Seq.foo to parallel version
// PSeq.foo. The functions preserve ordering of values only if you
// insert 'PSeq.ordered' at the beginning (which is necessary in
// this example!)

// Turn on time measurements
#time 

let windowData = slidingWindow 200
let valueData = values 100

// TODO: Modify 'slidingWindow' function from the previous section

// ------------------------------------------------------------------
// Visualizing all the data using single chart

FSharpChart.Combine
  [ // Draw range chart with standard deviation and nice color
    windowData
    |> Array.map (fun (dt, v,sdv) -> dt, (v - sdv, v + sdv))
    |> FSharpChart.Range
    |> FSharpChart.WithSeries.Style
        ( Color = Color.FromArgb(32, 135, 206, 250), 
          BorderColor = Color.SkyBlue, BorderWidth = 1)  
    
    // Draw line chart with actual values
    valueData |> FSharpChart.Line 

    // Draw line chart with averaged values
    windowData |> Array.map (fun (dt, v, _) -> dt, v)
    |> FSharpChart.Line ]



// ------------------------------------------------------------------
// Processing Live data from NASDAQ 
// ------------------------------------------------------------------

#r "bin\\\FSharp.AsyncExtensions.dll"
#load "bin\\LiveData.fsx"

open System
open LiveData

// ----------------------------------------------------------------------------
// NASDAQ - download data from NASDAQ - this gets new value every second
// but the data source does not update that frequently...

let d = 
  getStockPrices "NASDAQ"
  |> Observable.subscribe (printfn "%A")

// Stop the processing
d.Dispose()

// Visualize live data using FSharpChart
getStockPrices "NASDAQ" |> FSharpChart.Line

// ==================================================================
// TASK #9
// Rewrite some of the examples from previous tasks to use live data
// (for example, calculate average value over a sliding window)

// (...)
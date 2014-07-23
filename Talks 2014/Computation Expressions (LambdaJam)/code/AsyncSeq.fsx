#load "lib/FSharp.AsyncSeq.fsx"
open System
open System.IO
open FSharp.AsyncSeq
open FSharp.Charting
open FSharp.Control

// --------------------------------------------------------
// Using asynchronous sequences
// --------------------------------------------------------

let file = "C:/data/msft.csv"

let data = asyncSeq {
  let reader = new StreamReader(file)
  for line in reader.AsyncReadLines() |> AsyncSeq.skip 1 do
    do! Async.Sleep(100)
    let parts = line.Split(',')
    let date, price = DateTime.Parse(parts.[0]), float parts.[1]
    yield date, price }

data |> LiveChart.NiceLine

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

let rec readLines (reader:StreamReader) = asyncSeq {
  let! line = reader.AsyncReadLine()
  if line <> null then
    yield line 
    yield! readLines (reader) }

let data = asyncSeq {
  use reader = new StreamReader(file)
  for line in readLines reader |> AsyncSeq.skip 1 do
    do! Async.Sleep(100)
    let parts = line.Split(',')
    yield DateTime.Parse(parts.[0]), float parts.[1] }

data |> LiveChart.NiceLine
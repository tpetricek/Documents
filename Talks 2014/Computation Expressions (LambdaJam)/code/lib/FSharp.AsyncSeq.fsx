#I "."
#load "FSharp.Charting.fsx"
#r "FSharp.AsyncExtensions.dll"
open FSharp.Charting
open FSharp.Control
open System
open System.Net
open System.Text
open FSharp.IO
open FSharp.Control
open System.IO

type System.IO.StreamReader with
  member x.AsyncReadLine() = 
    x.ReadLineAsync() |> Async.AwaitTask

type FSharp.Charting.LiveChart with
  static member NiceLine(data) = 
    let grid = ChartTypes.Grid(LineColor=System.Drawing.Color.LightGray)
    data 
    |> AsyncSeq.toObservable
    |> Observable.windowed 20
    |> LiveChart.Line
    |> Chart.WithYAxis(Min=20.0, Max=45.0)
    |> Chart.WithYAxis(MajorGrid=grid)
    |> Chart.WithXAxis(MajorGrid=grid)

type System.IO.StreamReader with
  member reader.AsyncReadLines() = asyncSeq {
    let! line = reader.AsyncReadLine()
    if line <> null then
      yield line 
      yield! reader.AsyncReadLines() }

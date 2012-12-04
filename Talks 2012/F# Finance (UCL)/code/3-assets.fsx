// Load the F# chart library and Math.NET library
#load "lib/FSharpChart-0.8d.fsx"
#r "lib/MathNet.Numerics.dll"
#r "lib/Samples.CsvProvider.dll"

open System
open Samples.FSharp.Charting
open Samples.FSharp.Charting.ChartStyles
open MathNet.Numerics.Distributions

// -------------------------------------------------------
// Type provider generated code...

type StockData = Samples.FSharp.CsvProvider.MiniCsv<"http://ichart.finance.yahoo.com/table.csv?s=FB">

let urlFor ticker (startDate:System.DateTime) (endDate:System.DateTime) = 
  sprintf "http://ichart.finance.yahoo.com/table.csv?s=%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" 
          ticker (startDate.Month - 1) startDate.Day startDate.Year 
          (endDate.Month - 1) endDate.Day endDate.Year

let stockData ticker startDate endDate = 
  StockData(urlFor ticker startDate endDate)

// -------------------------------------------------------
// Working with Math.NET distributions

let dist1 = Normal(0.0, 1.0, RandomSource = Random(100))
dist1.Sample()

// USE IN EARLIER TUTORIAL

[ for n in 0 .. 10000 -> dist1.Sample() ]
|> Seq.map (fun v -> Math.Round(v * 5.0), v)
|> Seq.groupBy fst
|> Seq.map (fun (k, v) -> k, Seq.length v)
|> Chart.Column


// -------------------------------------------------------
// Generating asset price movement - simple random walk

module Chart = 
  let SimpleLine(seq, count) =
    seq |> Seq.take count
        |> Seq.mapi (fun i v -> i, v)
        |> Chart.Line

let dist = Normal(0.0, 1.0, RandomSource = Random(100))
dist.Sample()

let rec randomWalk value = seq { 
  yield value 
  yield! randomWalk (value + dist.Sample()) }

Chart.SimpleLine(randomWalk 10.0, 500)

// -------------------------------------------------------
// Generating asset price movement - better stuff

let randomPrice drift volatility dt initial (dist:Normal) = 
  let driftExp = (drift - 0.5 * pown volatility 2) * dt
  let randExp = volatility * (sqrt dt)
  let rec loop price = seq {
    yield price
    let price = price * exp (driftExp + randExp * dist.Sample()) 
    yield! loop price }
  loop initial

// let drift = 0.05     // mu / drift 0.01 - 0.1
// let volatility = 0.2 // sigma / volatility 0.05 - 0.5

// Plot a sample
let dist = Normal(0.0, 1.0, RandomSource = Random(100))
Chart.SimpleLine(randomPrice 0.05 0.05 0.005 5.0 dist, 500)

// Exercise: Explore how different volatility and drift affect charts
let dist1 = Normal(0.0, 1.0, RandomSource = Random(100))
let dist2 = Normal(0.0, 1.0, RandomSource = Random(100))

Chart.Combine
  [ Chart.SimpleLine(randomPrice 0.05 0.10 0.001 5.0 dist1, 500) 
    Chart.SimpleLine(randomPrice 0.10 0.50 0.001 5.0 dist2, 500) ]

// -------------------------------------------------------

let msft2012 = stockData "MSFT" (DateTime(2012,1,1)) DateTime.Now

let first = msft2012.Data |> Seq.minBy (fun itm -> itm.Date)

let fakeMsft drift volatility = 
  let dist = Normal(0.0, 1.0)
  let dates = [ for v in msft2012.Data -> v.Date ] |> List.rev
  let randoms = randomPrice drift volatility 0.005 first.Close dist
  Seq.zip dates randoms

Chart.Line (fakeMsft 0.01 0.05)
Chart.Line [ for item in msft2012.Data -> item.Date, item.Close ]

// Exercise: Experiment to find the volatility and drift for MSFT
Chart.Combine
  [ Chart.Line(fakeMsft 0.05 0.1, Name = "Fake")
    Chart.Line([ for item in msft2012.Data -> item.Date, item.Close ], Name = "Real") ]

// -------------------------------------------------------
// Calculate the volatility and drift from historical data

let divs = 
  msft2012.Data 
  |> Seq.sortBy (fun v -> v.Date)
  |> Seq.pairwise
  |> Seq.map (fun (prev, next) -> log (next.Close / prev.Close))

let stats = MathNet.Numerics.Statistics.DescriptiveStatistics(divs)
let tau = 1.0 / 252.0 // 1/number of trading days

// Values 'divs' follow normal distribution with properties:
// * mean = (drift - (pown volatility 2)/2.0) * tau 
// * variance = (pown volatility 2) * tau
let volatilityMsft = sqrt (stats.Variance / tau)

let driftMsft = (stats.Mean / tau) + (pown volatilityMsft 2) / 2.0

[ for a in 0 .. 2 ->
  [ for b in 0 .. 2 ->
    let i = a * 3 + b
    Chart.Combine
      [ Chart.FastLine(fakeMsft driftMsft volatilityMsft, Name = sprintf "Fake %d" i )
        Chart.FastLine([ for item in msft2012.Data -> item.Date, item.Close ], Name = sprintf "Real %d" i) ] ]
  |> Chart.Rows ]
|> Chart.Columns

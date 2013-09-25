#I "lib"
#r "FSharp.Data.dll"
#r "FCell.XlProvider.dll"
#r "MathNet.Numerics.dll"
#load "lib/FSharp.DataFrame.fsx"
#load "lib/FSharp.Charting.fsx"
#load "Utils.fs"

open System
open FSharp.Data
open FSharp.Charting
open FSharp.DataFrame
open MathNet.Numerics.Distributions
open FCell.TypeProviders.XlProvider

// ----------------------------------------------------------------------------
// Tutorial #3: Working with (randomly generated!) stock data 
// ----------------------------------------------------------------------------


// Generate price using geometric Brownian motion
let randomPrice drift volatility initial count span = 
  let dist = Normal(0.0, 1.0, RandomSource=Random(0))  
  let dt = 1.0 / (250.0 * 24.0 * 60.0)
  let driftExp = (drift - 0.5 * pown volatility 2) * dt
  let randExp = volatility * (sqrt dt)
  (DateTimeOffset(DateTime(2013, 1, 1)), initial) |> Seq.unfold (fun (dt, price) ->
    let price = price * exp (driftExp + randExp * dist.Sample()) 
    let dt = dt + span
    Some((dt, price), (dt, price))) |> Seq.take count

// Generate two "high-frequency" time series (with different volatility)
let hfq1 = Series.ofObservations (randomPrice 0.05 0.1 20.0 (24*60*60) (TimeSpan(0, 0, 1)))
let hfq2 = Series.ofObservations (randomPrice -0.05 0.3 20.0 (24*60*60) (TimeSpan(0, 0, 1)))

// Chart them using F# Chart to see what they look like
Chart.Combine(
  [ Chart.FastLine(Series.observations hfq1)
    Chart.FastLine(Series.observations hfq2) ]).WithYAxis(Min=15.0, Max=25.0)
  
// Calculate the means of the two series 
hfq1 |> Series.mean
hfq2 |> Series.mean

// ----------------------------------------------------------------------------

// TASK #1: We have a series with data for two months and one observation per 1 sec.
// We want to get data in 1 minute resolution (and just throw away the remaining
// observations). Then we want to calculate the pairwise difference between 
// their logarithms...

// TODO: Generate 1 minute intervals for 1 montth, then
// lookup nearest smaller value for each second & take log,
// then calculate pairwise differences
// 
// Hint: Series.lookupAll, log, Series.pairwiseWith 
let intervals = [ DateTimeOffset(DateTime(2013, 1, 1)) ]

let logs1 = Series.ofValues [0]
let diffs = Series.ofValues [0]

// We can compare the two series in a chart
Chart.Rows 
  [ Chart.Line(logs1 |> Series.observations)
    Chart.Line(diffs |> Series.observations) ]


// TASK #2: Chunk the data into 1 hour blocks and calculate
// the means and standard deviations for each chunk. Then
// show two charts showing the means/sdvs.
//
// Hints:
//   Series.chunkDist - lets you specify distance between keys
//     and returns chunks based on the distance. Alternatively,
//     Series.groupBy will do too.
//   Series.withOrdinalIndex - given series with some index (like dates)
//     returns a new series with auto-integer indexed series
//     (e.g. if you want to drop the index)
//   Frame.ofColumns - create frame from a sequence of columns
//   Frame.sdv & Frame.mean - stats!

// TODO: Visualize the data nicely in Excel
ActiveWorkbook.Stocks <- !! (Frame.ofColumns ["Data" => diffs])
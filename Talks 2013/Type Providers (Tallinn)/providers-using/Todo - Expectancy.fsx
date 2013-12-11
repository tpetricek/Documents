#r "System.Xml.Linq.dll"
#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"
open FSharp.Net
open FSharp.Data

open System
open FSharp.Data
open FSharp.Charting
open Deedle

// ----------------------------------------------------------------------------
// Tutorial #2: Looking at life expectancy in WorldBank
// ----------------------------------------------------------------------------

// Get data for some countries in WorldBank
let wb = WorldBankData.GetDataContext()

let us  = wb.Countries.``United States``.Indicators.``Life expectancy at birth, total (years)`` |> series
let uk  = wb.Countries.``United Kingdom``.Indicators.``Life expectancy at birth, total (years)`` |> series
let jpn = wb.Countries.Japan.Indicators.``Life expectancy at birth, total (years)`` |> series
let chi = wb.Countries.China.Indicators.``Life expectancy at birth, total (years)`` |> series
let eg  = wb.Countries.``Egypt, Arab Rep.``.Indicators.``Life expectancy at birth, total (years)`` |> series
let es  = wb.Countries.Estonia.Indicators.``Life expectancy at birth, total (years)`` |> series

let expectations = 
  frame [ "UK" => uk; "US" => us; "Japan" => jpn; "China" => chi; "Egypt" => eg; "Estonia" => es ]

// Draw a quick line chart
Chart.Line(expectations?UK)

// Average UK and US
Chart.Line((expectations?UK + expectations?US) / 2.0)

// Chart that shows all countries
let lines =
  expectations.Columns
  |> Series.observations
  |> Seq.map (fun (name, values) -> Chart.Line(values.As<float>(), name))
Chart.Combine(lines).WithLegend(Docking=ChartTypes.Docking.Left).WithYAxis(Min=40.0)

// ----------------------------------------------------------------------------
// TASK #1: Compare life expectancies for male & female
// (or compare different baltic countries :-))
// (for any country you fancy) and compare their growths

// HINTS:
// Get data from WorldBank & chrate combined chart (for male & female)
Chart.Combine
  [ Chart.Line(expectations?Estonia)
    Chart.Line(expectations?UK) ]

// ----------------------------------------------------------------------------
// TASK #2: Calculate the growth speed for life expectancy
// (you can do that for Estonia or for all countries)
// & visualize the differences

// HINTS:

// If we create a new series, we can add it as follows
let newSeries = expectations?Estonia / 10.0
expectations?Test <- newSeries

// If we have an ordered series, we can use "pairwise" function
let s = series [ 1 => 1.0; 2 => 2.0; 3 => 3.0 ]
s |> Series.pairwiseWith (fun k (prev, next) -> (prev + next) / 2.0)


// ----------------------------------------------------------------------------
// TASK #3: Pick a country (e.g. Estionia) and calculate the 
// difference in average life expectancy for each decade.

// HINTS:

// Given a series, we can use chunk by to create chunks
let sn = series [ for i in 1 .. 100 -> i => sin ((float i) / 100.0) ]

// Create chunks such that the difference between keys is less than 10
sn |> Series.chunkWhile (fun k1 k2 -> k2 - k1 < 10)

// ... again, then apply averaginig on the chunk (a chunk
// is a nested series)
sn 
|> Series.chunkWhile (fun k1 k2 -> k2 - k1 < 10)
|> Series.map (fun k v -> Series.mean v)


#I "lib"
#r "FSharp.Data.dll"
#r "FCell.XlProvider.dll"
#load "lib/FSharp.DataFrame.fsx"
#load "lib/FSharp.Charting.fsx"
#load "Utils.fs"

open System
open FSharp.Data
open FSharp.Charting
open FSharp.DataFrame
open FCell.TypeProviders.XlProvider

// ----------------------------------------------------------------------------
// Tutorial #2: Looking at life expectancy in WorldBank
// ----------------------------------------------------------------------------

// Get data for some countries in WorldBank
let wb = WorldBankData.GetDataContext()

let us  = wb.Countries.``United States``.Indicators.``Life expectancy at birth, total (years)`` |> Series.ofObservations
let uk  = wb.Countries.``United Kingdom``.Indicators.``Life expectancy at birth, total (years)`` |> Series.ofObservations
let jpn = wb.Countries.Japan.Indicators.``Life expectancy at birth, total (years)`` |> Series.ofObservations
let chi = wb.Countries.China.Indicators.``Life expectancy at birth, total (years)`` |> Series.ofObservations
let eg  = wb.Countries.``Egypt, Arab Rep.``.Indicators.``Life expectancy at birth, total (years)`` |> Series.ofObservations

let expectations = Frame.ofColumns [ "UK" => uk; "US" => us; "Japan" => jpn; "China" => chi; "Egypt" => eg ]

// Draw a quick line chart
expectations?UK |> Series.observations |> Chart.Line

// Average UK and US
((expectations?UK + expectations?US) / 2.0) |> Series.observations |> Chart.Line

// Chart that shows all countries
let lines =
  expectations.Columns
  |> Series.observations
  |> Seq.map (fun (name, values) -> 
      let data = values.As<float>() |> Series.observations
      Chart.Line(data, name))
Chart.Combine(lines).WithLegend(Docking=ChartTypes.Docking.Left).WithYAxis(Min=40.0)

// Pass data to Excel for other processing
ActiveWorkbook.LifeExpectancy <- !!expectations
ActiveWorkbook.LifeExpectancyKeys <- array2D [ for k in expectations.RowKeys -> [float k]]

// TASK #1: Pick a country (e.g. US) and calculate the 
// difference in average life expectancy for each decade.
// Store the results in a nice Excel spreadsheet!
//
// TASK #2: Calculate the growth speed for life expectancy
// (you can do that for US or for all countries)
// Store the growth speeds in Excel spreadsheet & visualize them!
//
// TASK #3: Compare life expectancies for male & female
// (for any country you fancy) and compare their growths
//
// Some useful functions you may need:
// 
//  frame.Rows & frame.Columns - return the rows/columns
//    of a data frame as a series (so that you can use
//    Series.<...> functions on rows/columns)
//  Series.chunk - create a series of non-overlapping chunks
//  Series.map - projection e.g. to get some data from a chunk
//  s.[k] - to get the value for a given key
//  Series.observations - to get all data in the series as key-value tuples
//  Series.mean - to calculate the average
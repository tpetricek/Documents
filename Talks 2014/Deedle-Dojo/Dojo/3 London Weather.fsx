#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"
open System
open Deedle
open FSharp.Net
open FSharp.Data
open FSharp.Data.Json
open FSharp.Charting

// ----------------------------------------------------------------------------
// Download UK PMs from Wikipedia
// ----------------------------------------------------------------------------

module DataAccess  =
  open FSharp.Data.Json.Extensions

  // Get London ID
  let londonId = JsonProvider<"http://api.openweathermap.org/data/2.5/history/city?q=London,UK">.GetSample().CityId

  // Converting to and from UNIX timestamps
  let unix = DateTime(1970,1,1,0,0,0,0, DateTimeKind.Utc)
  let fromUnixTime unixTimeStamp = unix.AddSeconds(float unixTimeStamp).ToLocalTime()
  let toUnixTime (dt:DateTime) = int (dt - unix.ToLocalTime()).TotalSeconds

  // Get data for January 2013 and January 2014
  type History = JsonProvider<"http://api.openweathermap.org/data/2.5/history/city?id=2885679&type=hour&start=1369728000&end=1369789200">
  let downloadDay city (dayFrom:DateTime) (dayTo:DateTime) =
    let appid = "1181b74e032049338cdc29312cdd294d"
    let start, endt = toUnixTime dayFrom, toUnixTime dayTo
    let hist = History.Load(sprintf "http://api.openweathermap.org/data/2.5/history/city?id=%d&unit=metric&type=day&start=%d&end=%d&APPID=%s" city start endt appid)
    hist.List

  let jan2013 = downloadDay londonId (DateTime(2013,1,1)) (DateTime(2013,1,31))
  let jan2014 = downloadDay londonId (DateTime(2014,1,1)) (DateTime(2014,1,31))

  // Extract interesting weather information from the historical records
  // This could be nicer if we tweaked the sample for the JSON provider to 
  // also include rain information (which is optional and uses odd format!)
  let getColumns (info:History.DomainTypes.List) =
    let dt = fromUnixTime info.Dt
    let rain =
      match info.JsonValue.TryGetProperty("rain"), info.JsonValue.TryGetProperty("forecast_rain") with
      | Some r, _ | _, Some r -> 
          match r.TryGetProperty("3h"), r.TryGetProperty("1h") with
          | Some h3, _ -> h3.AsFloat() / 3.0
          | _, Some h1 -> h1.AsFloat()
          | _ -> Double.NaN
      | _ -> Double.NaN 
    dt, series [ "Temp" => box (info.Main.Temp - 273.15M); 
                 "Date" => box dt
                 "Clouds" => box info.Clouds.All;
                 "Wind" => box info.Wind.Speed 
                 "Pressure" => box info.Main.Pressure
                 "Rain" => box rain ]

// Get two data frames and save them locally
let df2013 = DataAccess.jan2013 |> Seq.map DataAccess.getColumns |> Frame.ofRows
let df2014 = DataAccess.jan2014 |> Seq.map DataAccess.getColumns |> Frame.ofRows 
df2013.SaveCsv(__SOURCE_DIRECTORY__ + "/data/london-jan2013.csv")
df2014.SaveCsv(__SOURCE_DIRECTORY__ + "/data/london-jan2014.csv")

// ============================================================================
// Weather
// ============================================================================

let weatherJan13 = 
  Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/data/london-jan2013.csv")
let weatherJan14 = 
  Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/data/london-jan2014.csv")

// ----------------------------------------------------------------------------
// TASK: Compare the average weather conditions in January 2013 and 2014
// ----------------------------------------------------------------------------

// HINT: Use 'Frame.mean' to get a quick summary of the information
// HINT: You can use various sampling functions to get averages per days
// (note that our input is fairly high-resolution)

// HINT: First, we need to index rows by date
let byDate13 = weatherJan13 |> Frame.indexRowsDate "Date"
let byDate14 = weatherJan14 |> Frame.indexRowsDate "Date"

// For example, sample data into 1 day slots and take averages
let daily13 = 
  byDate13?Temp 
  |> Series.sampleTimeInto (TimeSpan(24,0,0)) Direction.Forward Series.mean

// HINT: Do some fun visualizations - crate a chart for single time series
Chart.Line(weatherJan14?Rain)
// HINT: Using sampled time series with DateTime keys is much nicer!
Chart.Line(daily13)

// HINT: You can combine charts with 'Chart.Combine' - but the following example
// is wrong, because the values are not aligned - you need to index them by DateTime
Chart.Combine
 [ Chart.Line(weatherJan14?Temp)
   Chart.Line(weatherJan13?Temp) ]
#load @"packages\FsLab.0.0.16\FsLab.fsx"
open Deedle
open FSharp.Data
open FSharp.Charting

// TODO: Calling simple REST based API
// 

type T = JsonProvider<""" [{"a":14}, {"a":1234.234, "b":true}] """>
for s in T.GetSamples() do
  s.

type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/weather?units=metric&q=Prague">

let w = Weather.Load("http://api.openweathermap.org/data/2.5/weather?units=metric&q=Brooklyn")
w.Main.Temp
let ww = 


// TODO: Using JsonProvider for type-safe calls

// TODO: Accessing weather information for another place

// TODO: Function that returns temperature in a given city
  
// DEMO: Combining information from multiple data sources

// TODO: Add temperature in the capital as an indicator

// TODO: Visualize using the R provider
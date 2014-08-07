#load @"packages\FsLab.0.0.16\FsLab.fsx"
open Deedle
open FSharp.Data
open FSharp.Charting

// TODO: Calling simple REST based API
// http://api.openweathermap.org/data/2.5/weather?units=metric&q=Prague
Http.Request("http://api.openweathermap.org/data/2.5/weather?units=metric&q=Prague").Body

// TODO: Using JsonProvider for type-safe calls
type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/weather?units=metric&q=Prague">

// TODO: Accessing weather information for another place
let bk = Weather.Load("http://api.openweathermap.org/data/2.5/weather?units=metric&q=Brooklyn")
bk.Sys.Country
bk.Main.Temp

// TODO: Function that returns temperature in a given city
let getTemp city = 
  let w = Weather.Load("http://api.openweathermap.org/data/2.5/weather?units=metric&q=" + city)
  w.Main.Temp
  
// DEMO: Combining information from multiple data sources
let wb = WorldBankData.GetDataContext()
let countries = [
  wb.Countries.China;
  wb.Countries.``United States``;
  wb.Countries.``United Kingdom``;
  wb.Countries.France;
  wb.Countries.``Czech Republic``
  wb.Countries.Brazil;
  wb.Countries.``Russian Federation``
  wb.Countries.Japan;
  wb.Countries.``South Africa``;
  wb.Countries.Germany;
  wb.Countries.Mexico; ]

let info = Frame.ofRowKeys [ for c in countries -> c.Name ]
info?GDP <- [ for c in countries -> c.Indicators.``GDP (current US$)``.[2000] ]
info?Growth <- [ for c in countries -> c.Indicators.``GDP per capita growth (annual %)``.[2000] ]
info?Population <- [ for c in countries -> c.Indicators.``Population (Total)``.[2000] ]
info?Pollution <- [ for c in countries -> c.Indicators.``CO2 emissions (kg per PPP $ of GDP)``.[2000] ]
info?Gender <- [ for c in countries -> c.Indicators.``Employment to population ratio, 15+, female (%)``.[2000] ]
// TODO: Add temperature in the capital as an indicator
info?Temp <- [ for c in countries -> getTemp c.CapitalCity ]

open RProvider.graphics
R.plot(info)

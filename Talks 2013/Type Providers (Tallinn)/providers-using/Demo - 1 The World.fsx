#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"
open Deedle
open FSharp.Data
open FSharp.Net
open FSharp.Charting

// ----------------------------------------------------------------------------
// Get information about countries & call temperature REST API
// ----------------------------------------------------------------------------

let wb = WorldBankData.GetDataContext()
let city = wb.Countries.``Czech Republic``.CapitalCity

type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/weather?units=metric&q=Prague">
let prague = Weather.Load("http://api.openweathermap.org/data/2.5/weather?units=metric&q=" + city)

prague.Main.Temp
prague.Sys.Country

let getTemp city = 
  let w = Weather.Load("http://api.openweathermap.org/data/2.5/weather?units=metric&q=" + city)
  w.Main.Temp

getTemp city

// ----------------------------------------------------------------------------
// Get information for multiple countries as a series
// ----------------------------------------------------------------------------

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

let temps = series [ for c in countries -> c.Name, getTemp c.CapitalCity ]

// ----------------------------------------------------------------------------
// Get additional information from the world bank
// ----------------------------------------------------------------------------

// Get GDP of Czech Republic in 2000
wb.Countries.``Czech Republic``.Indicators.``GDP (current US$)``.[2000]

// Create data frame 
let info = frame [ "Temp" => temps ]

// Add data for other countries
info?GDP <- [ for c in countries -> c.Indicators.``GDP (current US$)``.[2000] ]
info?Growth <- [ for c in countries -> c.Indicators.``GDP per capita growth (annual %)``.[2000] ]
info?Population <- [ for c in countries -> c.Indicators.``Population (Total)``.[2000] ]
info?Pollution <- [ for c in countries -> c.Indicators.``CO2 emissions (kg per PPP $ of GDP)``.[2000] ]
info?Gender <- [ for c in countries -> c.Indicators.``Employment to population ratio, 15+, female (%)``.[2000] ]

// ----------------------------------------------------------------------------
// Visualize data with the R type provider
// ----------------------------------------------------------------------------

open RProvider.graphics

R.plot(info)

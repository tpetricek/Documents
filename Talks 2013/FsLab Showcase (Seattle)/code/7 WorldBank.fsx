#load "FsLab.fsx"
#load "Vega/Vega.fsx"
#load "Excel/Excel.fsx"

open FsLab
open Deedle
open VegaHub
open FSharp.Data

// Accessing a CSV file...
type dataset = CsvProvider<"train.csv">
let data = new dataset()

let first = data.Data |> Seq.head

// Now I got strongly-typed Passengers

// Accessing World Bank data, over the wire
let wb = WorldBankData.GetDataContext()
wb.Countries.Bhutan.CapitalCity
let pop = series wb.Countries.Italy.Indicators.``Population (Total)``
Vega.Scatter(frame ["Italy" => pop], "Italy")

wb.Countries.``Lao PDR``.CapitalCity
wb.Countries.``Czech Republic``.CapitalCity
// I get discoverability via IntelliSense
wb.Countries
wb.Countries.China.Indicators

let large = 
    query {
        for c in wb.Countries do
        where (c.Indicators.``Population (Total)``.[2000] > 100000000.)
        select c.Name }
    |> Seq.iter (printfn "%s")

// Using R with static typing?
open RDotNet
open RProvider
open RProvider.``base``
open RProvider.graphics

// Packages and operations are discoverable

// And I can use F# data manipulation + R
[ 1 .. 10 ] |> R.plot

let china = wb.Countries.China
let gdp = [ for y in 2000 .. 2010 -> china.Indicators.``GDP (current US$)``.[y]]
gdp |> R.barplot

let countries = [
    wb.Countries.China;
    wb.Countries.``United States``;
    wb.Countries.``United Kingdom``;
    wb.Countries.France;
    wb.Countries.Brazil;
    wb.Countries.Japan;
    wb.Countries.``South Africa``;
    wb.Countries.Germany;
    wb.Countries.Mexico; ]

let gdp2000 = [ for c in countries -> c.Indicators.``GDP (current US$)``.[2000] ]
let gro2000 = [ for c in countries -> c.Indicators.``GDP per capita growth (annual %)``.[2000] ]
let pop2000 = [ for c in countries -> c.Indicators.``Population (Total)``.[2000] ]
let pol2000 = [ for c in countries -> c.Indicators.``CO2 emissions (kg per PPP $ of GDP)``.[2000] ]
let gen2000 = [ for c in countries -> c.Indicators.``Employment to population ratio, 15+, female (%)``.[2000] ]

[ "GDP", gdp2000;
  "Growth", gro2000;
  "Pop", pop2000;
  "Pollution", pol2000;
  "gender", gen2000 ]
|> namedParams
|> R.data_frame
|> R.plot



open FSharp.Net

let doc = Http.Request("http://api.openweathermap.org/data/2.5/weather?q=London,uk&units=metric")

type Weather = JsonProvider<"""{"coord":{"lon":-0.13,"lat":51.51},"sys":{"message":0.096,"country":"GB","sunrise":1384759488,"sunset":1384790810},"weather":[{"id":803,"main":"Clouds","description":"broken clouds","icon":"04n"}],"base":"gdps stations","main":{"temp":280.57,"pressure":1009,"humidity":81,"temp_min":280.15,"temp_max":281.15},"wind":{"speed":4.1,"deg":300},"rain":{"3h":0},"clouds":{"all":80},"dt":1384815000,"id":2643743,"name":"London","cod":200}""">

Weather.Parse(doc).Main.Temp
Weather.Parse(doc).Name

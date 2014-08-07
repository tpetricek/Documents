#load @"packages\FsLab.0.0.16\FsLab.fsx"
open Deedle
open FSharp.Data
open FSharp.Charting

// TODO: Visualizing university enrollment in the US
let wb = WorldBankData.GetDataContext()

wb.Countries.``United States``.Indicators.``School enrollment, tertiary (% gross)``
|> List.ofSeq
|> Chart.Line


// TODO: Comarping university enrollment for male/female students

Chart.Combine
  [ Chart.Line(wb.Countries.``United States``.Indicators.``School enrollment, tertiary, male (% gross)``, Name="Male")
    Chart.Line(wb.Countries.``United States``.Indicators.``School enrollment, tertiary, female (% gross)``, Name="Female") ]
|> Chart.WithLegend()


// DEMO: Creating a list of countries
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

// TODO: Creating a data frame with indicators

let info = Frame.ofRowKeys [ for c in countries -> c.Name  ]
info?GDP <- [ for c in countries -> c.Indicators.``GDP (current US$)``.[2000] ] 
info?Growth <- [ for c in countries -> c.Indicators.``GDP per capita growth (annual %)``.[2000] ]
info?Population <- [ for c in countries -> c.Indicators.``Population (Total)``.[2000] ]
info?Pollution <- [ for c in countries -> c.Indicators.``CO2 emissions (kg per PPP $ of GDP)``.[2000] ]
info?Gender <- [ for c in countries -> c.Indicators.``Employment to population ratio, 15+, female (%)``.[2000] ]


// DEMO: Adding additional indicators

// TODO: Looking at correlations using the R provider

open RProvider.graphics

R.plot(info)


// ---------------------------------------------------------
// Load type provider for World Bank and F# charting library
// ---------------------------------------------------------

#r "lib/Samples.WorldBank.dll"
#load "lib/FSharpChart-0.8d.fsx"
open Samples.FSharp.Charting
open Samples.FSharp.Charting.ChartStyles

// ---------------------------------------------------------
// Explore university enrollment
// ---------------------------------------------------------

// Create data context connected to WorldBank
let data = Samples.WorldBank.GetDataContext()

let cze = data.Countries.``Czech Republic``
let cz = data.Countries.``Czech Republic``.Indicators.``School enrollment, tertiary (% gross)``
let dk = data.Countries.Denmark.Indicators.``School enrollment, tertiary (% gross)``

Chart.Combine
  [ Chart.Line(cz, Name="CZ")
    Chart.Line(dk, Name="dk") ]










    

// ---------------------------------------------------------
// Compare university enrolment with OECD
// ---------------------------------------------------------

// Calculate average data for all OECD members
let oecd =
  [ for c in data.Regions.``OECD members``.Countries do
      yield! c.Indicators.``School enrollment, tertiary (% gross)`` ]
  |> Seq.groupBy fst
  |> Seq.map (fun (y, v) -> y, Seq.averageBy snd v)
  |> Array.ofSeq

Chart.Combine
  [ Chart.Line(oecd, Name="OECD")
    Chart.Line(cz, Name="CZ")
    Chart.Line(dk, Name="dk") ]


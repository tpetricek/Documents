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

// Get university enrollment of CZ
data.Countries.``Czech Republic``.Indicators.``School enrollment, tertiary (% gross)``

// Plot the enrollment as a line chart
let cz = data.Countries.``Czech Republic``.Indicators.``School enrollment, tertiary (% gross)``
Chart.Line(cz, Name = "Czech Republic")

// Compare the enrollment in CZ and DK
let cz = data.Countries.``Czech Republic``.Indicators.``School enrollment, tertiary (% gross)``
let dk = data.Countries.Denmark.Indicators.``School enrollment, tertiary (% gross)``
    
Chart.Combine [ Chart.Line(cz, Name="Czech Republic"); Chart.Line(dk, Name="Denmark") ]

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

// Select countries that we want to compare
let countries = 
  [ data.Countries.``Czech Republic``; data.Countries.Denmark
    data.Countries.``United Kingdom``; data.Countries.``United States`` ]

// Create an aggregate chart
Chart.Combine 
  [ yield Chart.Line(oecd, Name="OECD")
    for c in countries do
      let data = c.Indicators.``School enrollment, tertiary (% gross)``
      yield Chart.Line(data, Name = c.Name) ]

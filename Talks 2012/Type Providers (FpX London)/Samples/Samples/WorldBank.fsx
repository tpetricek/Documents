#r @"..\Providers\WorldBank.TypeProvider\bin\Debug\WorldBank.TypeProvider.dll"
#load "FSharpChartEx.fsx"
open MSDN.FSharp.Charting
open System.Drawing

// -----------------------------------------------------------------

// Get average university enrollment for EU countries
let avgEU =
    [ for c in WorldBank.Regions.``European Union`` do
        yield! c.``School enrollment, tertiary (% gross)`` ]
    |> Seq.groupBy fst
    |> Seq.map (fun (y, v) -> y, Seq.averageBy snd v)
    |> Array.ofSeq
    |> Array.sortBy fst

// Create list of other countries that we want to plot
let countries = 
  [ WorldBank.Countries.``Czech Republic``, Color.DarkRed
    WorldBank.Countries.Norway, Color.DarkGreen
    WorldBank.Countries.``United Kingdom``, Color.Black
    WorldBank.Countries.Estonia, Color.DarkGoldenrod ]

// Compare countries with EU average
FSharpChart.Combine
  [ yield FSharpChart.NiceLine(avgEU, "EU", Color.Blue)
    for country, clr in countries do
      let data = country.``School enrollment, tertiary (% gross)``
      yield FSharpChart.NiceLine(data, unbox country, clr) ]
|> FSharpChart.NiceChart
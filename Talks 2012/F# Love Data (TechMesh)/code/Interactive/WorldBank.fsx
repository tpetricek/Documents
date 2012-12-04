#load "FSharpChart.fsx"
#r "../References/WorldBank.TypeProvider.dll"

open FSharp.Data
open FSharp.Charting

// ------------------------------------------------------------------
// World bank countries

type WorldBank = WorldBankProvider<"World Development Indicators", Asynchronous=true>
let data = WorldBank.GetDataContext()

let countries = 
 [| data.Countries.``Arab World`` 
    data.Countries.``European Union``
    data.Countries.Australia
    data.Countries.Brazil
    data.Countries.Canada
    data.Countries.Chile
    data.Countries.``Czech Republic``
    data.Countries.Denmark
    data.Countries.France
    data.Countries.Greece
    data.Countries.``Low income``
    data.Countries.``High income``
    data.Countries.``United Kingdom``
    data.Countries.``United States`` |]

// ----------------------------------------------------------------------------
// Create University enrollment chart

[ for c in countries -> 
    c.Indicators.``School enrollment, tertiary (% gross)`` ]
|> Async.Parallel
|> Async.RunSynchronously
|> Array.map Chart.Line
|> Chart.Combine

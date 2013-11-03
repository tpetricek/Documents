#r "../packages/FSharp.Data.1.1.10/lib/net40/FSharp.Data.dll"
#load "../packages/FSharp.Charting.0.87/FSharp.Charting.fsx"

open FSharp.Data
open FSharp.Charting

// ------------------------------------------------------------------
// World bank countries

type WorldBank = WorldBankDataProvider<"World Development Indicators", Asynchronous=true>
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

#r @"packages\FSharp.Data.1.1.1\lib\net40\FSharp.Data.dll"
#load @"..\References\FSharpChart.fsx"

open FSharp.Data
open FSharp.Charting

type WorldBank = WorldBankDataProvider<Asynchronous=true>
let data = WorldBank.GetDataContext()

// Create a list of countries for comparing various indicators
let countries = 
  [ data.Countries.``Arab World`` 
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
    data.Countries.``United States`` ]

// Calculate average school enrollment from a list of countries (in parallel!)
[ for c in countries -> c.Indicators.``School enrollment, tertiary (% gross)`` ]
|> Async.Parallel
|> Async.RunSynchronously
|> Seq.concat
|> Seq.groupBy fst
|> Seq.choose (fun (k, vs) ->
    if Seq.length vs < Seq.length countries - 2 then None
    else Some(k, Seq.averageBy snd vs))
|> Chart.Line
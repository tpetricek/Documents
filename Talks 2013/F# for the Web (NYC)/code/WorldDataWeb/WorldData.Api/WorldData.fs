#if INTERACTIVE
#r "../../packages/FSharp.Data.1.1.10/lib/net40/FSharp.Data.dll"
#else
module FsWeb.WorldData
#endif
open FSharp.Data

// Display population in EURO area countries
let wb = WorldBankData.GetDataContext()
let populationInEuroArea () =
  [ for c in wb.Regions.``Euro area``.Countries ->
      let _, population = c.Indicators.``Population, total`` |> Seq.maxBy fst 
      c.Name, population ]

// Display population in top 20 world countries & the rest
let populationInOECD () =
  [ for c in wb.Regions.``OECD members``.Countries ->
      let _, population = c.Indicators.``Population, total`` |> Seq.maxBy fst 
      c.Name, population ]

// Get population in different US states
type US = CsvProvider<"us-states.csv", InferRows=10, IgnoreErrors=true>
let liveUrl = "http://visualizing.org/sites/default/files/data_set/admin/NST_EST2009_ALLDATA.csv"
let populationInUSA () =
  let live = US.Load(liveUrl)
  [ for row in live.Data do
      if row.SUMLEV = 40 then
        yield row.NAME, row.CENSUS2000POP ]

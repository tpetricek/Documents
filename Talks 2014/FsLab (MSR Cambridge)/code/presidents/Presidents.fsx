#I ".."
#load @"..\packages\FsLab.0.0.18\FsLab.fsx"
#load @"vega\Vega.fsx"
open Deedle
open VegaHub
open System
open System.Linq
open FSharp.Data
open FSharp.Charting

// ----------------------------------------------------------------------------
// Getting debt data
// ----------------------------------------------------------------------------

// Loading US debt data from CSV file
type Debt = CsvProvider<"C:/data/us-debt.csv">
let data = Debt.GetSample()

// Creating Deedle series and frames
let debt = series [ for r in data.Rows -> r.Year => r.``Debt (percent GDP)`` ]
let gdp = series [ for r in data.Rows -> r.Year => r.``GDP-US $ billion nominal`` ]
let gov = frame [ "Debt" => debt; "GDP" => gdp ]

Chart.Line(gov?Debt)

// Compare the GDP data with what we can get from WorldBank
let wb = WorldBankData.GetDataContext()
let wbGdp = series wb.Countries.``United States``.Indicators.``GDP (current US$)``

// Compare data from us-debt CSV file and WorldBank
gov?GDP_WB <- wbGdp / 1.0e9

Chart.Combine
  [ Chart.Line(gov?GDP)
    Chart.Line(gov?GDP_WB) ]

// ----------------------------------------------------------------------------
// Get information about US presidents from Freebase
// ----------------------------------------------------------------------------

// Get president list from Freebase!
type FreebaseData = FreebaseDataProvider<"AIzaSyCEgJTvxPjj6zqLKTK06nPYkUooSO1tB0w">
let fb = FreebaseData.GetDataContext()

fb.Society.Government.``US Presidents`` |> List.ofSeq

// When were they presidents & party information
let abe = fb.Society.Government.``US Presidents``.First()

abe.Name
abe.``Government Positions Held``.First().``Basic title``
abe.``Government Positions Held``.First().From
abe.``Government Positions Held``.First().To

[ for p in abe.Party -> p.Party.Name ]

// Get presidents since around 1900
let presidentInfos = 
  query { for p in fb.Society.Government.``US Presidents`` do
          sortBy (Seq.max p.``President number``)
          skip 23 }

// Get list of pres
let presidentTerms =
  [ for pres in presidentInfos do
    for pos in pres.``Government Positions Held`` do
    if string pos.``Basic title`` = "President" then
      // Get start and end year of the position
      let starty = DateTime.Parse(pos.From).Year
      let endy = if pos.To = null then 2013 else
                   DateTime.Parse(pos.To).Year
      // Get their party
      let dem = pres.Party |> Seq.exists (fun p -> p.Party.Name = "Democratic Party")
      let rep = pres.Party |> Seq.exists (fun p -> p.Party.Name = "Republican Party")
      let pty = if rep then "Republican" elif dem then "Democrat" else null
      // Return three element tuple with the info
      yield (pres.Name, starty, endy, pty) ]

// Build a data frame containing the information
let presidents =
  presidentTerms
  |> Frame.ofRecords
  |> Frame.indexColsWith ["President"; "Start"; "End"; "Party"]

// ----------------------------------------------------------------------------
// Analysing debt change during presidential terms
// ----------------------------------------------------------------------------

// Get yearly debt with the ruling president
let byStart = presidents |> Frame.indexRowsInt "Start"
let yearly = gov.Join(byStart, JoinKind.Left, Lookup.ExactOrSmaller)

// Display debt change with president & party
Vega.BarColor(yearly, y="Debt", color="President")
Vega.BarColor(yearly, y="Debt", color="Party")

// Calculate change for each president
yearly?Difference <-
  yearly?Debt |> Series.pairwiseWith (fun _ (prev, curr) -> curr - prev)

// Difference by president
Vega.BarColor(yearly, y="Difference", color="President")
Vega.BarColor(yearly, y="Difference", color="Party")

// Compare Republican and Democratic presidents
let debtByParty = 
  yearly
  |> Frame.groupRowsByString "Party"
  |> Frame.getCol "Debt"
  |> Stats.levelMean fst

Chart.Pie(debtByParty)
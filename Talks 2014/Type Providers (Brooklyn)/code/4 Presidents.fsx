#load @"packages\FsLab.0.0.16\FsLab.fsx"
#load @"vega\Vega.fsx"
open Deedle
open VegaHub
open FSharp.Data
open System
open System.Linq

// ----------------------------------------------------------------------------
// Getting debt data
// ----------------------------------------------------------------------------

// Loading US debt data from CSV file & making the data set nicer
let debtData = 
  Frame.ReadCsv("c:/data/us-debt.csv")
  |> Frame.indexRowsInt "Year"
  |> Frame.indexColsWith ["GDP"; "Population"; "Debt"; "Source" ]

// Compare the GDP data with what we can get from WorldBank
let wb = WorldBankData.GetDataContext()

let wbGdp = series wb.Countries.``United States``.Indicators.``GDP (current US$)``

// Compare data from us-debt CSV file and WorldBank
debtData?GDP_WB <- wbGdp / 1.0e9

[ for (KeyValue(year, v)) in debtData?GDP.Observations  do
    yield year, v, "UsDebt", 50.0
  for (KeyValue(year, v)) in debtData?GDP_WB.Observations do
    yield year, v, "WorldBank", 20.0 ]
|> Vega.Scatter

// Open web browser with the visualization
Vega.Open()

// For the rest of the analysis, we need just the Debt
let debt = debtData.Columns.[ ["Debt"] ]

Vega.Scatter(debt, "Debt")

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
// Analysing debt change per year
// ----------------------------------------------------------------------------

// For each year, find the corresponding president and party
let byStart = presidents |> Frame.indexRowsInt "Start"
byStart?Start <- byStart.RowKeys
let aligned = debt.Join(byStart, JoinKind.Left, Lookup.ExactOrSmaller)


Vega.BarColor(aligned, y="Debt", color="President")  
Vega.BarColor(aligned, y="Debt", color="Party") 



let byParty = 
  aligned 
  |> Frame.groupRowsByString "Party"

let meanChange =
  byParty?Debt 
  |> Series.diff 1
  |> Stats.levelMean fst

Vega.BarColor(frame ["Change" => meanChange ], y="Change")

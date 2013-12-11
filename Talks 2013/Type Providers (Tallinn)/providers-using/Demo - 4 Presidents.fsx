#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"
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
  Frame.ReadCsv("C:/data/us-debt.csv")
  |> Frame.indexRowsInt "Year"
  |> Frame.indexColsWith ["Year"; "GDP"; "Population"; "Debt"; "?" ]

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
      let pty = if dem then "Democrat" elif rep then "Republican" else null
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

// Get debt at the end of the presidential term
let byEnd = presidents |> Frame.indexRowsInt "End"
let endDebt = byEnd.Join(debt, JoinKind.Left)

// Total accumulated debt by president
Vega.BarColor(endDebt, x="End", y="Debt", color="President")
Vega.BarColor(endDebt, x="End", y="Debt", color="Party")


// ----------------------------------------------------------------------------
// Analysing average debt change (per term, by party)
// ----------------------------------------------------------------------------

// Calculate change for each president
endDebt?Difference <-
  endDebt?Debt 
  |> Series.pairwiseWith (fun _ (prev, curr) -> curr - prev)

// Difference by president
Vega.BarColor(endDebt, x="End", y="Difference", color="President")
Vega.BarColor(endDebt, x="End", y="Difference", color="Party")

// Compare Republican and Democratic presidents
let debtByParty = 
  endDebt 
  |> Frame.groupRowsByString "Party"
  |> Frame.meanLevel fst
debtByParty?Party <- debtByParty.RowKeys

Vega.BarColor(debtByParty, "Difference", color="Party")

// ----------------------------------------------------------------------------
// Analysing debt change per year
// ----------------------------------------------------------------------------

// For each year, find the corresponding president and party
let byStart = presidents |> Frame.indexRowsInt "Start"
let aligned = debt.Join(byStart, JoinKind.Left, Lookup.NearestSmaller)

// Bar chart displaying debt per president per year 
Vega.BarColor(aligned, y="Debt", color="President")  

// Bar chart displaying debt per party per year 
Vega.BarColor(aligned, y="Debt", color="Party") 



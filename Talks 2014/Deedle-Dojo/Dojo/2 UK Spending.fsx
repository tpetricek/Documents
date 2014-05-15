#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"
#load @"vega\Vega.fsx"
open System
open VegaHub
open Deedle
open FSharp.Net
open FSharp.Data
open FSharp.Charting
open System.Text.RegularExpressions

// ----------------------------------------------------------------------------
// Download UK PMs from Wikipedia
// ----------------------------------------------------------------------------

// Explicit list of known parties to make parsing simpler &
// Download information from Wikipedia page
let parties = 
  [ "National Labour"; "Conservative"; "Labour"; 
    "Liberal"; "Whig"; "Peelite"; "Tory" ]
let wiki = Http.Request("http://en.wikipedia.org/wiki/List_of_Prime_Ministers_of_the_United_Kingdom")

// Dirty prasing using regular expressions. This will get much nicer once
// we add HTML <table> type provider to the F# Data relase :-) Coming soon!
let ministersWiki = 
  [ for m in Regex.Matches(wiki, """\<a [^\>]*\>(?<first>[^\<]*) \<b\>(?<second>[^\<]*)</b>\<\/a\>""") ->
      let name = m.Groups.["first"].Value + " " + m.Groups.["second"].Value
      let afterName = wiki.Substring(m.Index)
      let year = Regex.Match(afterName, """\<td\>\<small\>(?<date>[^\<]*)\<\/small\>\<br \/\>\s*(?<year>[0-9]+)\<\/td\>\s*\<td\>""")
      let party = parties |> Seq.minBy (fun p -> 
        let i = afterName.IndexOf(p) in if i = -1 then System.Int32.MaxValue else i)
      let date = year.Groups.["date"].Value + " " + year.Groups.["year"].Value
      series [ "Name" => name; "Date" => date; "Party" => party ] ]
  |> Frame.ofRowsOrdinal

// Save the data locally, in case Wikipedia format changes
ministersWiki.SaveCsv(__SOURCE_DIRECTORY__ + "/data/uk-ministers.csv")

// ----------------------------------------------------------------------------
// Download some information from the WorldBank
// (sadly, WorldBank only has debt information for the last few years,
// but you can extend this code to get other fun things!)
// ----------------------------------------------------------------------------

// Get a couple of data series from the WorldBank
let wb = WorldBankData.GetDataContext()
let indicatorsWb =
  [ "GDP" => series wb.Countries.``United Kingdom``.Indicators.``GDP per capita (current US$)``
    "University" => series wb.Countries.``United Kingdom``.Indicators.``School enrollment, tertiary (% gross)`` ]
  |> Frame.ofColumns

// Save the data locally, in case the internet is down!
indicatorsWb?Year <- Seq.map (fun y -> DateTime(y, 12, 31)) indicatorsWb.RowKeys
indicatorsWb.SaveCsv(__SOURCE_DIRECTORY__ + "/data/uk-indicators.csv")

// ============================================================================
// Relating UK government debt (etc.) with PMs and their political parties 
// ============================================================================

let ministers = 
  Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/data/uk-ministers.csv")
  |> Frame.indexRowsDate "Date"

let spending =
  Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/data/uk-spending.csv")
  |> Frame.indexColsWith ["Year"; "GDP"; "Population"; "Debt"; "?"]
  |> Frame.dropSeries "GDP"
  |> Frame.indexRowsInt "Year"
  |> Frame.mapRowKeys (fun y -> DateTime(y, 12, 31))

let indicators = 
  Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/data/uk-indicators.csv")
  |> Frame.indexRowsDate "Year"

// ----------------------------------------------------------------------------
// TASK #1: How many times was each of the parties the ruling party? 
// Count this over the entire data set and also just over the last 50 years
// ----------------------------------------------------------------------------

// Given a data frame, get specified column as a series of the given type
ministers.GetSeries<string>("Party")

// Get values of a series (throw away keys) as an ordinary seq<'T> type
ministers.GetSeries<string>("Party").Values

// You can slice a data frame using low/high bound on the key
// (Note: works only for sorted frames, the key does not have to be there)
ministers.Rows.[DateTime(1990, 1, 1) ..]

// HINT: You can use Seq.countBy
// HINT: Create a nice chart with Chart.Pie
// HINT: Use slicing to get ministers after 1/1/1950
// HINT: Create another nice Pie chart and compare the two 
// (most frequently elected party over all times vs. last 50 years)


// ----------------------------------------------------------------------------
// TASK #2: Exploring how different PMs and different political parties
// change the UK government debt. We need to join 'ministers' and 'spending'
// and then do some analysis & visualization over the resulting frame
// ----------------------------------------------------------------------------

// Fancy slicing - get a frame containing only the columns
// specified in the list passed to the indexer (returns a *Frame*)
let debt = spending.Columns.[ ["Debt"] ]

// HINT: Here are a few ways to join 'ministers' and 'debt' frames
// find the one that gives the right results!

// Inner join (keys have to match exactly) 
ministers.Join(debt, JoinKind.Inner)
// Left join (add matching value from 'debt', keys have to match exactly)
ministers.Join(debt, JoinKind.Left)
// Left join, when key is not found, find previous or next
ministers.Join(debt, JoinKind.Left, Lookup.NearestGreater)
ministers.Join(debt, JoinKind.Left, Lookup.NearestSmaller)

// Left join, but now we attach PM information to the years
debt.Join(ministers, JoinKind.Left, Lookup.NearestGreater)
debt.Join(ministers, JoinKind.Left, Lookup.NearestSmaller)

// HINT: If this would not add duplicate keys, you can transform
// keys from being DateTime to just Year using Frame.mapRowKeys

// HINT: You can use 'Frame.dropSparseRows' to throw away rows that 
// have some missing values (we do not want to deal with NaNs)

// HINT: Create a nice chart using 'Vega.BarColor'. 
// You can pass a data frame to Vega charting directly. The parameter
// "y" sets the column with Y values and you can optionally set 
// "color" to specify the category (such as PM name or Party :-))
Vega.BarColor(spending, y="Population")
Vega.BarColor(spending, y="Population", color="Year")

// Open web browser with the chart
Vega.Open()

// HINT: Calculate the difference in the Debt between two years
// (or between two PMs). This can be done using 'Series.pairwiseWith'
// The following example shows how to calculate population growth
let growth = 
  spending?Population 
  |> Series.pairwiseWith (fun k (prev, next) -> next - prev)

// We can add this back to the data frame as a new series
// (and then we can build a nice Vega chart!)
spending?PopulationGrowth <- growth
Vega.BarColor(spending, y="PopulationGrowth")
Vega.BarColor(spending, y="PopulationGrowth", color="Year")

// BONUS: As a bonus point, you can now group the rows by Party
// and calcluate the average difference in Debt per party. To do this
// use 'Frame.groupRowsByString' (and pass it "Party" as the column)


#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"

open Deedle
open FSharp.Charting

// ----------------------------------------------------------------------------
// Exploring Titanic disaster
// ----------------------------------------------------------------------------

let titanic = Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/../data/Titanic.csv")

// Count number of surivals and deaths
titanic
|> Frame.filterRows (fun _ row -> row.GetAs "Survived")
|> Frame.countRows

titanic
|> Frame.filterRows (fun _ row -> not (row.GetAs "Survived"))
|> Frame.countRows

// ----------------------------------------------------------------------------
// Working with multilevel indices
// ----------------------------------------------------------------------------

let byClass = 
  titanic
  |> Frame.groupRowsByInt "Pclass"

// Mean & sum everything by class 
byClass
|> Frame.meanLevel fst

// Look at survived column as booleans
let survivedByClass = byClass.GetSeries<bool>("Survived")

// Count number of survived/died in each group
let survivals = 
  survivedByClass
  |> Series.applyLevel fst (fun sr -> 
      sr.Values |> Seq.countBy id |> series)
  |> Frame.ofRows
  |> Frame.indexColsWith ["Died"; "Survived"]

// Count total number of passangers in each group
survivals?Total <- byClass |> Frame.applyLevel fst Series.countKeys

survivals

let summary = 
  [ "Survived (%)" => survivals?Survived / survivals?Total * 100.0
    "Died (%)" => survivals?Died/ survivals?Total * 100.0 ] |> frame
  |> round

Chart.Columns
  [ for k in summary.RowKeys -> 
      Chart.Pie(summary.Rows.[k].As<float>()).WithTitle(string k) ]

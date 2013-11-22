#load "FsLab.fsx"

open FsLab
open System
open Deedle
open RProvider
open FSharp.Data
open FSharp.Charting

// ----------------------------------------------------------------------------
// Exploring Titanic disaster
// ----------------------------------------------------------------------------

let titanic = Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/data/Titanic.csv")

// Count number of surivals and deaths
titanic
|> Frame.filterRows (fun _ row -> row.GetAs<bool>("Survived"))
|> Frame.countRows

titanic
|> Frame.filterRows (fun _ row -> not (row.GetAs<bool>("Survived")))
|> Frame.countRows

// Group by survival and get average statistics
titanic
|> Frame.groupRowsUsing (fun _ row -> (row.GetAs<bool>("Survived")))
|> Frame.meanLevel fst

// Draw simple pie chart
titanic
|> Frame.groupRowsUsing (fun _ row -> (row.GetAs<bool>("Survived")))
|> Frame.countLevel fst
|> Frame.getSeries "PassengerId"
|> Series.indexWith ["Died"; "Survived"]
|> Chart.Pie

// ----------------------------------------------------------------------------
// Working with multikevek indices
// ----------------------------------------------------------------------------

let byClassAndPort = 
  titanic
  |> Frame.groupRowsByInt "Pclass"
  |> Frame.groupRowsByString "Embarked"
  |> Frame.mapRowKeys Pair.flatten3

// Get the age column
let ageByClassAndPort = byClassAndPort?Age

ageByClassAndPort |> Series.meanLevel Pair.get1And2Of3
ageByClassAndPort |> Series.countLevel Pair.get1And2Of3

// Mean & sum everything by class and port
byClassAndPort
|> Frame.meanLevel Pair.get1And2Of3

// Look at survived column as booleans
let survivedByClassAndPort = byClassAndPort.Columns.["Survived"].As<bool>()

// Count number of survived/died in each group
let survivals = 
  survivedByClassAndPort 
  |> Series.applyLevel Pair.get1And2Of3 (fun sr -> 
      sr.Values |> Seq.countBy id |> series)
  |> Frame.ofRows
  |> Frame.indexColsWith ["Survived"; "Died"]

// Count total number of passangers in each group
survivals?Total <- 
  byClassAndPort
  |> Frame.applyLevel Pair.get1And2Of3 Series.countKeys

survivals

let summary = 
  [ "Survived (%)" => survivals?Survived / survivals?Total * 100.0
    "Died (%)" => survivals?Died/ survivals?Total * 100.0 ] |> frame

round summary

summary |> Frame.meanLevel fst |> round
summary |> Frame.meanLevel snd |> round
  

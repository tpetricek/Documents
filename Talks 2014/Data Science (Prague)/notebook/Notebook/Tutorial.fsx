(*** hide ***)
(* BUILD (Ctrl+Shift+B) the project to restore NuGet packages first! *)
#I ".."
#load "packages/FsLab.0.0.13-beta/FsLab.fsx"
(**

Analysing Titanic data
======================
*)

open Deedle
open FSharp.Charting

let titanic = Frame.ReadCsv("C:/data/Titanic.csv")

(*** define-output: survivals ***)
titanic
|> Frame.filterRows (fun _ row -> row.GetAs "Survived")
|> Frame.countRows
(*** include-it: survivals ***)

(*** define-output: deaths ***)
titanic
|> Frame.filterRows (fun _ row -> not (row.GetAs "Survived"))
|> Frame.countRows
(*** include-it: deaths ***)

(**

More stuff:

*)
let byClass = 
  titanic
  |> Frame.groupRowsByInt "Pclass"

let ages =
  byClass?Age
  |> Stats.levelMean fst

(*** include-value: ages ***)

// Look at survived column as booleans
let survivedByClass = byClass.GetColumn<bool>("Survived")

// Count number of survived/died in each group
let survivals = 
  survivedByClass
  |> Series.applyLevel fst (fun sr -> 
      sr.Values |> Seq.countBy id |> series)
  |> Frame.ofRows
  |> Frame.indexColsWith ["Died"; "Survived"]

// Count total number of passangers in each group
survivals?Total <- byClass?PassengerId |> Series.applyLevel fst Series.countKeys

survivals
(*** include-value:survivals ***)

let summary = 
  [ "Survived (%)" => survivals?Survived / survivals?Total * 100.0
    "Died (%)" => survivals?Died/ survivals?Total * 100.0 ] |> frame
  |> round

(*** include-value:summary ***)

(*** define-output:chart ***)
Chart.Columns
  [ for k in summary.RowKeys -> 
      Chart.Pie(summary.Rows.[k].As<float>()).WithTitle(string k) ]
(*** include-it:chart ***)
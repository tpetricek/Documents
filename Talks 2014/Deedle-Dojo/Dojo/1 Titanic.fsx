#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"

open Deedle
open FSharp.Charting

// ----------------------------------------------------------------------------
// Exploring Titanic disaster
// ----------------------------------------------------------------------------

let titanic = Frame.ReadCsv(__SOURCE_DIRECTORY__ + "/data/Titanic.csv")

// ----------------------------------------------------------------------------
// TASK #1: Find out how likely is person to survive based on their
// age group. You can take age groups 0-10, 10-20, 20-30 etc.
//
// To do this, you can mostly adapt the code from "1 Titanic Demo.fs"
// (rather than grouping by Pclass, group by a new column AgeGroup that 
// you calculate and add based on the Age column).
// ----------------------------------------------------------------------------

// To get a series as a floating point value series:
titanic?Age
// To get a series wiht a specific type:
titanic.GetSeries<int>("Age")

// To perform some calculation over an entire series:
titanic?Age * 100.0
// To apply a function to all values of a series
titanic?Age |> Series.mapValues (fun v -> int v)

// To add a new series to an existing data frame
titanic?AgeTwice <- titanic?Age * 2.0

// To create a frame with multi-level index based on Pclass
let byClass = titanic |> Frame.groupRowsByInt "Pclass"

// To get a series of a multi-level indexed frame (same as earlier)
let survivedByClass = byClass.GetSeries<bool>("Survived")

// Count number of survived/died in each group
let survivals = 
  survivedByClass
  |> Series.applyLevel fst (fun sr -> 
      sr.Values |> Seq.countBy id |> series)
  |> Frame.ofRows
  |> Frame.indexColsWith ["Died"; "Survived"]

// To iterate over all rows (key is class, value is a row of the frame)
// and generate pie chart for each single row & combine as multi-column chart
[ for (KeyValue(k,r)) in survivals.Rows.Observations ->
    Chart.Pie(r.As<float>()).WithTitle(sprintf "Class %d" k) ]
|> Chart.Columns
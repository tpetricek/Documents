#load @"packages\FsLab.0.0.16\FsLab.fsx"
open Deedle
open FSharp.Data
open FSharp.Charting

// TOOD: Loading titanic data set from C:/data/titanic.csv
type Titanic = CsvProvider<"C:/data/titanic.csv">
let titanic = Titanic.GetSample()

// TODO: Print all passangers with their age
for row in titanic.Rows do
  printfn "%s (%f)" row.Name row.Age

// TODO: Count the number of (not) surviving passangers
titanic.Rows
|> Seq.filter (fun p -> p.Survived)
|> Seq.length

// TODO: Comparing survival ratio based on passanger class
titanic.Rows
|> Seq.groupBy (fun r -> r.Pclass)
|> Seq.map (fun (pc, rows) ->
      pc, rows |> Seq.countBy (fun r -> r.Survived) |> dict)
|> Seq.sortBy fst
|> Seq.map (fun (pc, d) ->
      Chart.Pie(["Survived", d.[true]; "Died", d.[false] ])
           .WithTitle(sprintf "Class %d" pc) )
|> Chart.Columns
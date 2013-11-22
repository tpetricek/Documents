#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"

open Deedle
open FSharp.Charting

// ----------------------------------------------------------------------------
// Exploring Titanic disaster
// ----------------------------------------------------------------------------

let titanic = Frame.ReadCsv("C:/data/Titanic.csv")

// TODO: Count the surival rate 

// ----------------------------------------------------------------------------
// Working with multilevel indices
// ----------------------------------------------------------------------------

// TODO: Group rows by PClass column
// TODO: Calculate mean of other columns

// TODO: Get the Survived series
// TODO: Count the number of survived in each group

// TODO: Summarize the data set
// TODO: Visualize rows as Pie charts

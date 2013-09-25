#I "lib"
#r "FSharp.Data.dll"
#r "FCell.XlProvider.dll"
#load "lib/FSharp.DataFrame.fsx"
#load "Utils.fs"

open System
open FSharp.Data
open FSharp.DataFrame
open FCell.TypeProviders.XlProvider

// ----------------------------------------------------------------------------
// Tutorial #1: Working with New York (state) mortaility data
// ----------------------------------------------------------------------------

let mortality = Frame.readCsv(__SOURCE_DIRECTORY__ + "/data/MortalityNY.tsv")

// Use 'GetSeries' to access individual columns. The data 
// will be converted to the type you specify.
mortality.GetSeries<string>("Cause of death")
|> Series.values
|> Seq.distinct

// Group rows by 'County' column (and then convert key to strings)
mortality 
|> Frame.groupRowsBy "County"
|> Series.mapKeys string

// Group rows by the 'Cause of death' column
let causes = mortality.GroupRowsBy<string>("Cause of death")

// 'cause' is a series that contains data frames as values
// We can get individual data frame (with rows for the given cause)
causes.Get("Low back pain")
causes.Get("Pulmonary mycobacterial infection")

// Get all the observations as key-value pairs
causes |> Series.observations

// Get row by its row index (integers by default)
let firstRow = mortality.Rows.[0]

// Get different columns from the series
// (? converts to float, so we need 'GetAs' for strings)
firstRow?Count
firstRow?Population
firstRow.GetAs<string>("Cause of death")


// TASK #1: Find the most common causes of death in each county
// TASK #2: Find causes of death that killed less than 10 people
//
// Some functions you will probably need:
//
//  Series.map - transform values in the series & return a new series
//  Frame.filterRows - filter rows of a frame
//  s.TryGetAs<T>(..) - given a series, get the specified item as T
//  Frame.ofRows - create frame from a series of series
//  Frame.maxRowBy - get the maximal row by column

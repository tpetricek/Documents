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
// Walkthrough #0: Basics of working with data frames & series
// ----------------------------------------------------------------------------

// Create series from observation
let prices = 
  Series.ofObservations
    [ DateTime(2013,1,1) => 10.0
      DateTime(2013,1,4) => 20.0
      DateTime(2013,1,8) => 30.0 ]

// Creating data frame with single column
let df = Frame.ofColumns [ "Price" => prices ]
// Adding new column to a series
df?Squared <- df?Price * df?Price


// Get value at a specified data
prices.[DateTime(2013,1,1)]

// Get a range (note, keys do not have to be present if the series is ordered)
prices.[DateTime(2013,1,1) .. DateTime(2013,1,5)]

// Get prices at specified dates
prices |> Series.getAll [DateTime(2013,1,1); DateTime(2013,1,4)]
prices.GetItems([DateTime(2013,1,1); DateTime(2013,1,4)])

// Get prices and use advanced lookup for keys not in the series
prices.GetItems([DateTime(2013,1,1); DateTime(2013,1,5)])
prices.GetItems([DateTime(2013,1,1); DateTime(2013,1,5)], Lookup.NearestSmaller)


// ----------------------------------------------------------------------------

// Create data frame from F# records
type Person = { Name:string; Age:int; Countries:string list; }

let people = 
  [ { Name = "Joe"; Age = 51; Countries = [ "UK"; "US"; "UK"] }
    { Name = "Tomas"; Age = 28; Countries = [ "CZ"; "UK"; "US"; "CZ" ] }
    { Name = "Eve"; Age = 2; Countries = [ "FR" ] }
    { Name = "Suzanne"; Age = 15; Countries = [ "US" ] } ]
  |> Frame.ofRecords

// Get a series of numeric type
people?Age
// Get a series of non-numeric type
let countries = people.GetSeries<string list>("Countries")


// New series with incremented ages
people?Age |> Series.map (fun k v -> v + 1.0)
people?Age + 1.0

// ----------------------------------------------------------------------------

// Floating windows & chunking
let st = Series.ofValues [ 'a' .. 'j' ]

// Floating window & converting windows back to data frame
st |> Series.window 2 
st |> Series.window 2 |> Frame.ofRows
st |> Series.window 2 |> Series.map (fun _ row -> Series.withOrdinalIndex row) |> Frame.ofRows

// Specifying more complex boundary behaviour
st |> Series.windowSize (5, Boundary.AtBeginning) |> Series.map (fun _ row -> Series.withOrdinalIndex row) |> Frame.ofRows

// ----------------------------------------------------------------------------
// Writing data frame to Excel worksheet 

let xys = 
    [ "X" => Series.ofValues [ 1.0 .. 100.0 ]
      "Y" => Series.ofValues [ for i in 1.0 .. 100.0 -> i * i ] ] 
    |> Frame.ofColumns 

ActiveWorkbook.Tests <- !!xys
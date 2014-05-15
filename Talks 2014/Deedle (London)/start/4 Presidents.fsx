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

// TODO: Read CSV from C:/Data/us-debt.csv
// TODO: Index by Year (as debtData)
// DEMO: Add nice names to columns


// TODO: Get US GDP in current $ from WorldBank
// TODO: Add as a new column, divide by 1.0e9


// DEMO: Compare the two data sources
// TODO: Get debt column only, draw scatter plot

// ----------------------------------------------------------------------------
// Get information about US presidents from Freebase
// ----------------------------------------------------------------------------

// Get president list from Freebase!
type FreebaseData = FreebaseDataProvider<"AIzaSyCEgJTvxPjj6zqLKTK06nPYkUooSO1tB0w">
let fb = FreebaseData.GetDataContext()

// TODO: Get Society - Government - Presidents
// TODO: Explore properties like Govt positions


// TODO: Query US presidents, sortBy number, skip 23 (presidentInfos)
// DEMO: Get list of presidents from Freebase


// TODO: Build a data frame ofRecords
// DEMO: Update column indices (as 'presidents')

// ----------------------------------------------------------------------------
// Analysing debt change during presidential terms
// ----------------------------------------------------------------------------

// DEMO: Get debt at the end of the term
// TODO: Visualize per president/party information


// ----------------------------------------------------------------------------
// Analysing average debt change (per term, by party)
// ----------------------------------------------------------------------------

// TODO: Calculate change using pairwiseWith
// TODO: Visualize difference per president/party

// DEMO: Compare difference by parties

// ----------------------------------------------------------------------------
// Analysing debt change per year
// ----------------------------------------------------------------------------

// DEMO: Add president info for each year
// DEMO: Visualize debt per year 

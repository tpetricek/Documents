#I ".."
#load @"..\packages\FsLab.0.0.17\FsLab.fsx"
#load @"vega\Vega.fsx"
open Deedle
open VegaHub
open System
open System.Linq
open FSharp.Data
open FSharp.Charting

// ----------------------------------------------------------------------------
// Getting debt data
// ----------------------------------------------------------------------------

// TODO: Use CSV type provider with C:/Data/us-debt.csv
// TODO: Create 'debt' and 'gdp' series & 'gov' frame
// TODO: Line chart with gov?Debt

// TODO: Get US GDP in current $ from WorldBank
// TODO: Add as a new column, divide by 1.0e9
// TODO: Compare the two data sources

// ----------------------------------------------------------------------------
// Get information about US presidents from Freebase
// ----------------------------------------------------------------------------

// DEMO: Use Freebase type provider

// TODO: Get Society - Government - Presidents
// TODO: Explore properties like Govt positions

// TODO: Query US presidents, sortBy number, skip 23 (presidentInfos)
// DEMO: Get list of presidents from Freebase

// TODO: Build a data frame ofRecords
// DEMO: Update column indices (as 'presidents')

// ----------------------------------------------------------------------------
// Analysing debt change during presidential terms
// ----------------------------------------------------------------------------

// TODO: Index 'presidents' by 'Start' year
// TODO: Join presidents with government debt ('yearly')
// TODO: Display debt change with president & party

// TODO: Calculate and plot difference (Series.pairwiseWith)
// DEMO: Compare Republicans and Democrats & 'Chart.Pie'
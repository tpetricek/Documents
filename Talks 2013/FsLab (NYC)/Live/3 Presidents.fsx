#load "FsLab.fsx"

open FsLab
open System
open System.Linq
open Deedle
open RProvider
open FSharp.Data
open FSharp.Charting

// ----------------------------------------------------------------------------
// Getting debt data
// ----------------------------------------------------------------------------

// Step #0: Read CSV data from: __SOURCE_DIRECTORY__ + "/data/us-debt.csv"
// Step #1: Compare GDP with WorldBank data 

// ----------------------------------------------------------------------------
// Get information about US presidents
// ----------------------------------------------------------------------------

// Step #2: Get list of US presidents from Freebase & look at their details
// Step #3: Get presidents with their term years & party as a data frame

// ----------------------------------------------------------------------------
// Analyising debt change with Deedle
// ----------------------------------------------------------------------------

// Step #4: Index presidents by term end & add debt information
// Step #5: Analyze the data
//  - Add difference since the previous president
//  - Index frame by president & get the debt & the difference
//  - Plot the average debt change by party
//  - Making nicer plots with R's ggplot2

// ----------------------------------------------------------------------------
// Plotting debt by president 
// ----------------------------------------------------------------------------

// Step #6: Align the other way round - for each year, get president
// Step #7: Plot area chart with different areas per president
//  - Build nicely formatted labels for the chart
//  - Create "chunks" by checking while the president is the same
//  - For each chunk, create area chart & compose them
//  - Do simpilar chart using ggplot2
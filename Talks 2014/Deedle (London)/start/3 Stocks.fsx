#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"

open FsLab
open System
open Deedle
open RProvider
open FSharp.Data
open FSharp.Charting

// ----------------------------------------------------------------------------
// Getting data
// ----------------------------------------------------------------------------

// TODO: Get data using CSV provider based on "c:/data/stocks/fb.csv" (Stocks)
// DEMO: Function to load data for a given stock

// TODO: Load "MSFT" data, get average, average since 2013/1/1, plot as line
// TODO: Calculate 5 day sliding window, compare with prices (line charts)

// TODO: Calculate daily returns (ms-shifted)/ms*100% (returns)
// TODO: Chart daily returns, average daily returns over a window


// ----------------------------------------------------------------------------
// Working with entire data frames
// ----------------------------------------------------------------------------

// DEMO: Get data frame with some technology stocks

// TODO: Drop rows with missing data
// TODO: Calculate daily returns: (all-shifted)/all*100% (techRet)

// TODO: Column chart with average returns per company
// TODO: Column chart with average returns per day (transpose)


// ----------------------------------------------------------------------------
// Hierarchical indexing
// ----------------------------------------------------------------------------
  
// DEMO: Create hierarchical data frame with 
// prices for stocks in multiple sectors

// TODO: Drop rows with missing data (stocksAll)
// TODO: Calculate daily returns: (all-shifted)/all*100% (stocksRet)

// TODO: Average daily returns (mean)
// TODO: Column chart per comapny 

// TODO: Average daily returns per sector (meanLevel)
// TODO: Column chart per sector

#load "FsLab.fsx"

open FsLab
open System
open Deedle
open RProvider
open FSharp.Data
open FSharp.Charting

// ----------------------------------------------------------------------------
// Getting data
// ----------------------------------------------------------------------------

// Step #0: Get CSV data based on "data/fb.csv"
// Step #1: Basic charting & statistics
//  - mean over entire history, mean & chart over 2013
//  - chart price against line chart with the mean
//  - floating window averages (Series.windowInto)
// Step #2: Calculate returns & average returns
//  - calculate: (p[t] - p[t-1]) / p[t] * 100.0


// ----------------------------------------------------------------------------
// Working with entire data frames
// ----------------------------------------------------------------------------

// Step #3: Get stock prices for multiple IT companies as frame
// Step #4: 
//   - Drop sparse rows, calculate & plot returns
//   - Average returns per company (Chart.Column)
//   - Average returns per day (transpose & Column)     

// ----------------------------------------------------------------------------
// Hierarchical indexing
// ----------------------------------------------------------------------------
  
// Step #5: Get data for companies across different sectors
// Step #6: Calculate daily returns, average per company & sector
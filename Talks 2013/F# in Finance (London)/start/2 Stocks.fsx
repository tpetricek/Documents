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

// TODO: CSV provider on "data/fb.csv"
// TODO: Get data as a series & process
// DEMO: Function to read stock series

// TODO: Mean over entire history
// TODO: Mean over 2013 (using slicing)
// TODO: Plot chart prices
// TODO: Add floating window & plot 

// TODO: Calculate daily returns 
//   (p[t] - p[t-1]) / p[t] * 100.0
// TODO: Mean daily returns over a period

// ----------------------------------------------------------------------------
// Working with entire data frames
// ----------------------------------------------------------------------------

// DEMO: Get data frame with some technology stocks
// TODO: Drop missing, calculate & plot returns
// TODO: Average returns per company (Chart.Column)
// TODO: Average daily returns (transpose)

// ----------------------------------------------------------------------------
// Hierarchical indexing
// ----------------------------------------------------------------------------
  
// DEMO: Get data for companies across different sectors
// TODO: Calculate daily returns, average per company & sector

// TODO: Chart per company (mean & mapKeys snd)
// TODO: Chart per sector (mean level)
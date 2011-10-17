open System

#load "1_Data.fsx"
open StockData

let aapl = loadStockPrices "aapl"
let msft = loadStockPrices "msft"

// ------------------------------------------------------------------
// Processing data using F#

// Average opening price over the last 10 days
aapl
|> Seq.filter (fun (dt, (o,h,l,c)) -> 
    (DateTime.Now - dt).TotalDays < 10.0)
|> Seq.map (fun (dt, (o,h,l,c)) -> o)
|> Seq.average

// ==================================================================
// TASK #4
// Find number of days when closing price is 
// larger than opening price by more than $5.

// (...)

// ==================================================================
// TASK #5
// Calculate standard deviation of the data (using the equation
// from slides). Useful functions are Seq.sum, Seq.average and
// sqrt to calculate square root.

// (...)

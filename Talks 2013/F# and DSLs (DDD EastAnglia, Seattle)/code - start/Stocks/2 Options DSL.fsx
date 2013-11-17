// -------------------------------------------------------
// Modeling stock options
// -------------------------------------------------------

// TODO
//   What is the model??

// TODO
//   ButterflySpread "MSFT" 10.0M 20.0M $
//   ButterflySpread "MSFT" 50.0M 60.0M

// TODO
//   Can we make it easier to use? 
//   (support * and $, add Sell function)





// -------------------------------------------------------
// Calculating payoff using current prices
// -------------------------------------------------------

#load "Helpers.fsx"
#r "lib\\FSharp.Data.dll"
open FSharp.Data


// DEMO
//   CsvProvider using lib/MSFT.csv as a sample
//   Write 'getPrice name' function
//   -> Download "http://ichart.finance.yahoo.com/table.csv?s=" + name
//   -> sort by date, get closing rice, last or default value
//   -> Helpers.memoize

// DEMO
//   Implement recursive payoff function

// DEMO
//   ButterflySpread "MSFT" 10.0M 50.0M |> payoff
//   ButterflySpread "MSFT" 0.0M 70.0M |> payoff
//   ButterflySpread "MSFT" 40.0M 60.0M |> payoff

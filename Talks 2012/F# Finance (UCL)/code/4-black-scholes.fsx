// Load the F# chart library and Math.NET library
#load "lib/FSharpChart-0.8d.fsx"
#r "lib/MathNet.Numerics.dll"
#r "lib/Samples.CsvProvider.dll"

open System
open Samples.FSharp.Charting
open Samples.FSharp.Charting.ChartStyles
open MathNet.Numerics.Distributions

// -------------------------------------------------------
// Type provider generated code...

type StockData = Samples.FSharp.CsvProvider.MiniCsv<"msft.csv"> //"http://ichart.finance.yahoo.com/table.csv?s=FB">

let urlFor ticker (startDate:System.DateTime) (endDate:System.DateTime) = 
  sprintf "http://ichart.finance.yahoo.com/table.csv?s=%s&a=%i&b=%i&c=%i&d=%i&e=%i&f=%i" 
          ticker (startDate.Month - 1) startDate.Day startDate.Year 
          (endDate.Month - 1) endDate.Day endDate.Year

let stockData ticker startDate endDate = 
  StockData(urlFor ticker startDate endDate)

// -------------------------------------------------------
// Black-Scholes equation


// Consists of exercise price and time to expiry
type OptionType =
  | Call 
  | Put 

type OptionInfo = 
  { ExercisePrice : float
    TimeToExpiry : float 
    Kind : OptionType }

type StockInfo = 
  { Volatility : float
    CurrentPrice : float }









let getStockProperties name = 
  // currentPrice, volatility
  { CurrentPrice = 26.96; Volatility = 0.2075772981 }

// -------------------------------------------------------

let normal = Normal()

let blackScholes rate stock option =
  if option.TimeToExpiry > 0.0 then
    let d1 = 
      ( log(stock.CurrentPrice / option.ExercisePrice) + 
        (rate + 0.5 * pown stock.Volatility 2) * option.TimeToExpiry ) /
      ( stock.Volatility * sqrt option.TimeToExpiry )
    let d2 = d1 - stock.Volatility * sqrt option.TimeToExpiry
    let N1 = normal.CumulativeDistribution(d1)
    let N2 = normal.CumulativeDistribution(d2)

    let e = option.ExercisePrice * exp (-rate * option.TimeToExpiry)
    let call = stock.CurrentPrice * N1 - e * N2
    match option.Kind with
    | Call -> call
    | Put -> call + e - stock.CurrentPrice
  else
    match option.Kind with
    | Call -> max (stock.CurrentPrice - option.ExercisePrice) 0.0
    | Put -> max (option.ExercisePrice - stock.CurrentPrice) 0.0


// -------------------------------------------------------
// Get some stock option prices

let stock = { Volatility = 0.3; CurrentPrice = 5.0 }
let option = { ExercisePrice = 4.0; TimeToExpiry = 1.0; Kind = Call }
blackScholes 0.05 stock option

let msftStock = getStockProperties "MSFT"
let msftOption = { ExercisePrice = 30.0; TimeToExpiry = 1.0; Kind = Call }
blackScholes 0.05 msftStock msftOption

// -------------------------------------------------------
// Epic charting

#time

let varyPrice time option = 
  [| for price in 0.0 .. 0.2 .. 200.0 ->
       { option with ExercisePrice = price } |]

let opts = 
  [| for time in 0.0 .. 0.5 .. 10.0 ->
       let option = 
          { ExercisePrice = 0.0
            TimeToExpiry = time
            Kind = Call }
       time, varyPrice time option |]

// TODO: Run this in parallel
let values =       
  opts
  |> Array.map (fun (time, opts) -> 
      time, opts |> Array.map (fun option -> 
        option.ExercisePrice, blackScholes 0.05 msftStock option))
  
Chart.Combine
  [ for time, prices in values ->
      Chart.FastLine prices ]

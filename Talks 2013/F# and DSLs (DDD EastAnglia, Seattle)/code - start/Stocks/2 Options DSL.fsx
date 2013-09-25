type Kind = Put | Call

type Option = 
  | Euro of Kind * string * decimal
  | Combine of Option * Option
  | Times of int * Option

let Sell(option) = Times(-1, option)

type Option with
  static member (*) (x:int, option:Option) =
    Times(x, option)     
  static member ($) (option1:Option, option2:Option) = 
    Combine(option1, option2)  

let ButterflySpread lowPrice highPrice  = 
  Euro(Call, "MSFT", lowPrice) $
  Euro(Call, "MSFT", highPrice) $
  -2 * Euro(Call, "MSFT", ((lowPrice + highPrice) / 2.0M))


ButterflySpread 10.0M 20.0M $
ButterflySpread 50.0M 60.0M


(*
  (EuroCall lowPrice actualPrice) +
  (EuroCall highPrice actualPrice) -
  2.0 * (EuroCall ((lowPrice + highPrice) / 2.0) actualPrice)
*)

// -------------------------------------------------------

#load "Helpers.fsx"
#r "lib\\FSharp.Data.dll"
open FSharp.Data

type Stocks = CsvProvider<"lib\\MSFT.csv">

let getPrice = Helpers.memoize (fun name ->
  let prices = Stocks.Load("http://ichart.finance.yahoo.com/table.csv?s=" + name)
  query { for p in prices.Data do
          sortByDescending p.Date
          select p.Close
          headOrDefault })

// TODO: payoff function
let rec payoff option = 
  match option with
  | Euro(Call, stockName, exercisePrice) ->
      let actualPrice = getPrice stockName
      max 0.0M (actualPrice - exercisePrice)
  | Euro(Put, stockName, exercisePrice) ->
      let actualPrice = getPrice stockName
      max 0.0M (exercisePrice -  actualPrice)
  | Combine(option1, option2) ->
      (payoff option1) + (payoff option2) 
  | Times(n, option) ->
      decimal n * (payoff option)


(ButterflySpread 10.0M 50.0M)
|> payoff




payoff (ButterflySpread "MSFT" 30.0M 60.0M)
payoff (ButterflySpread "MSFT" 20.0M 50.0M)
payoff (ButterflySpread "MSFT" 0.0M 70.0M)

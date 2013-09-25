
type OptionKind = Put | Call
type StockTicker = string

type Option = 
  | European of OptionKind * StockTicker * decimal 
  | Combine of Option * Option
  | Times of int * Option

type Option with
  static member (*) (x:int, option:Option) =
    Times(x, option)     
  static member ($) (option1:Option, option2:Option) = 
    Combine(option1, option2)  

let Sell option = Times(-1, option)

let Strangle name lowPrice highPrice = 
  European(Put, name, lowPrice) $
  European(Put, name, highPrice)

let ButterflySpread name lowPrice highPrice = 
  European(Call, name, lowPrice) $
  European(Call, name, highPrice) $
  Sell(2 * European(Call, name, (lowPrice + highPrice) / 2.0M))
  
// -------------------------------------------------------

#load "Helpers.fsx"
#r "lib\\FSharp.Data.dll"
open FSharp.Data

type Stocks = CsvProvider<"lib\\MSFT.csv">

let getPrice = Helpers.memoize (fun name ->
  let data = Stocks.Load("http://ichart.finance.yahoo.com/table.csv?s=" + name)
  query { for r in data.Data do
          sortByDescending r.Date
          select r.Close
          headOrDefault })

let rec payoff = function
  | European(kind, name, exercisePrice) ->
      let actualPrice = getPrice name
      match kind with 
      | Call -> max (actualPrice - exercisePrice) 0.0M
      | Put -> max (exercisePrice - actualPrice) 0.0M
  | Combine(left, right) ->
      (payoff left) + (payoff right)
  | Times(r, option) ->
      decimal r * (payoff option)


payoff (ButterflySpread "MSFT" 30.0M 60.0M)
payoff (ButterflySpread "MSFT" 20.0M 50.0M)
payoff (ButterflySpread "MSFT" 0.0M 70.0M)

open System

// ------------------------------------------------------------------

/// Defines how a contract can be constructed
type Contract = 
  | Trade of string * float
  | Opposite of Contract
  | After of DateTime * Contract
  | Until of DateTime * Contract
  | Combine of Contract * Contract

/// Evaluate contract on a specific day
let rec run contract (day:DateTime) = seq {
  match contract with 
  | Trade(what, amount) -> 
      yield what, amount
  | Opposite(contract) ->
      for what, amount in run contract day do
        yield what, amount * -1.0 
  | After(dt, contract) ->
      if day >= dt then yield! run contract day
  | Until(dt, contract) ->
      if day <= dt then yield! run contract day
  | Combine(contract1, contract2) ->
      yield! run contract1 day
      yield! run contract2 day }

// Functions for creating basic contracts
let trade (what, amount) = Trade(what, amount)
let after dt contract = After(dt, contract)
let until dt contract = Until(dt, contract)
let opposite contract = Opposite(contract)
let ($) c1 c2 = Combine(c1, c2)

// Functions for creating advanced contracts
let purchase (what, amount) = trade(what, amount)
let sell (what, amount) = trade(what, amount) |> opposite
let onDate dt contract = after dt (until dt contract)
let repeatedly (start:DateTime) (span:TimeSpan) times contract = 
  [ for n in 0 .. times -> 
      onDate (start + TimeSpan(span.Ticks * int64 n)) contract ]
  |> Seq.reduce ($)
let purchaseOn date what = onDate date (purchase what)
let sellOn date what = onDate date (sell what)
let purchaseRepeatedly dt sp n what = repeatedly dt sp n (purchase what)

// ------------------------------------------------------------------
// Example - evaluating contracts

let msft = purchaseOn (DateTime(2012, 4, 21)) ("MSFT", 350.0)

let itcontract =   
   sellOn 
     (DateTime(2012, 4, 30)) ("MSFT", 23.0) $
   purchaseRepeatedly 
     (DateTime(2012, 4, 23)) (TimeSpan.FromDays(7.0)) 
     10 ("AAPL", 220.0)
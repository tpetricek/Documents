// Load the F# chart library or whatever is used on the web
#load "lib/FSharpChart-0.8d.fsx"
open Samples.FSharp.Charting
open Samples.FSharp.Charting.ChartStyles

// -------------------------------------------------------

// Function that specifies the gain
// of an European call option
let euroCallValue exercisePrice actualPrice = 
  max (actualPrice - exercisePrice) 0.0

// Function that specifies the gain
// of an European put option
let euroPutValue exercisePrice actualPrice =
  max (exercisePrice - actualPrice) 0.0

// -------------------------------------------------------

// Calculate the payoff of a given option 
// (option is represented as a function)
let optionPayoff option = 
  [ for p in 0.0 .. 10.0 .. 100.0 -> p, option p ]

// Compare the payoff of call and put options
let callPayoff = optionPayoff (euroCallValue 30.0)
let putPayoff = optionPayoff (euroPutValue 70.0)

// Draw the payoff diagrams
Chart.Columns
  [ Chart.Line callPayoff
    Chart.Line putPayoff ]

// -------------------------------------------------------

let bottomStraddle exercisePrice actualPrice = 
  (euroCallValue exercisePrice actualPrice) +
  (euroPutValue exercisePrice actualPrice) 

bottomStraddle 30.0 |> optionPayoff |> Chart.Line

let butterflySpread lowPrice highPrice actualPrice = 
  (euroCallValue lowPrice actualPrice) +
  (euroCallValue highPrice actualPrice) -
  2.0 * (euroCallValue ((lowPrice + highPrice) / 2.0) actualPrice)

butterflySpread 20.0 80.0 |> optionPayoff |> Chart.Line

// -------------------------------------------------------
  
// Exercise: Create bull spread
let bullSpread lowPrice highPrice actualPrice = 
  (euroCallValue lowPrice actualPrice) -
  (euroCallValue highPrice actualPrice) 
  
bullSpread 20.0 80.0 |> optionPayoff |> Chart.Line

// Exercise: Create strangle
let strangle lowPrice highPrice actualPrice = 
  (euroPutValue lowPrice actualPrice) +
  (euroCallValue highPrice actualPrice) 
  
strangle 20.0 80.0 |> optionPayoff |> Chart.Line

// -------------------------------------------------------

// Demo: Plot all four interesting options
let options = 
  [ [ "Bottom straddle", bottomStraddle 30.0; 
      "Butterfly spread", butterflySpread 20.0 80.0 ]
    [ "Bull spread", bullSpread 20.0 80.0 
      "Strangle", strangle 20.0 80.0 ] ]

Chart.Rows
  [ for suboptions in options ->
      [ for title, option in suboptions ->
          Chart.Line(optionPayoff option).AndTitle(title) ]
      |> Chart.Columns ]

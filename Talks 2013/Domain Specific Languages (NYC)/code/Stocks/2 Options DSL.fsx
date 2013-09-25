#load "lib/FSharpChart.fsx"
open Samples.FSharp.Charting
open Samples.FSharp.Charting.ChartStyles

type OptionKind = 
  | Call | Put
  
type Option = 
  | European of OptionKind * float 
  | Combine of Option * Option
  | Times of float * Option
  static member (*) (x:float, option:Option) =
    Times(x, option)     
  
let rec payoff actualPrice = function
  | European(Call, exercisePrice) ->
      max (actualPrice - exercisePrice) 0.0
  | European(Put, exercisePrice) -> 
      max (exercisePrice - actualPrice) 0.0
  | Combine(left, right) ->
      (payoff actualPrice left) + (payoff actualPrice right)
  | Times(r, option) ->
      r * (payoff actualPrice option)
      
let ($) opt1 opt2 = Combine(opt1, opt2)

  
/// Calculate the payoff of a given option 
let optionPayoff option = 
  [ for p in 0.0 .. 10.0 .. 100.0 -> p, payoff p option ]

// Compare the payoff of call and put options
let callPayoff = optionPayoff (European(Call, 30.0))
let putPayoff = optionPayoff (European(Put, 70.0))

let BullSpread(lowPrice, highPrice) = 
  European(Call, lowPrice) $ 
  -1.0 * European(Call, highPrice) 

BullSpread(30.0, 60.0) |> optionPayoff |> Chart.Line
  
let Strangle(lowPrice, highPrice) = 
  European(Put, lowPrice) $ 
  European(Call, highPrice)

let BottomStraddle(exercisePrice) = 
  European(Call, exercisePrice) $
  European(Put, exercisePrice) 
    
let ButterflySpread(lowPrice, highPrice) = 
  European(Call, lowPrice) $
  European(Call, highPrice) $
  -2.0 * (European(Call, (lowPrice + highPrice) / 2.0))
  

// Create a list of interesting options
let options = 
  [ [ "Bottom straddle", BottomStraddle(30.0)
      "Butterfly spread", ButterflySpread(20.0, 80.0) ]
    [ "Bull spread", BullSpread(20.0, 80.0)
      "Strangle", Strangle(20.0, 80.0) ] ]

// Combine the option payoffs in a single diagram
let grid = ChartTypes.Grid(LineColor=System.Drawing.Color.LightGray)

Chart.Rows
  [ for suboptions in options ->
      [ for title, option in suboptions ->
          Chart.Line(optionPayoff option).AndTitle(title).
                AndXAxis(MajorGrid=grid).AndYAxis(MajorGrid=grid) ]
      |> Chart.Columns ]

#load "lib/FSharpChart.fsx"
open Samples.FSharp.Charting
open Samples.FSharp.Charting.ChartStyles

// -------------------------------------------------------
// Helper function to get nice option chart
// -------------------------------------------------------

let PlotOption(option) =
  let grid = ChartTypes.Grid(LineColor=System.Drawing.Color.LightGray)
  let payoffs = [ for p in 0.0 .. 100.0 -> p, option p ]
  Chart.Line(payoffs).AndXAxis(MajorGrid=grid).AndYAxis(MajorGrid=grid)

// -------------------------------------------------------

let EuroCall exercisePrice actualPrice = 
  // Option to buy a comodity for 'exercisePrice'
  max 0.0 (actualPrice - exercisePrice)


let EuroPut exercisePrice actualPrice =
  // Option to sell a comodity for 'exercisePrice'
  max 0.0 (exercisePrice - actualPrice)


EuroCall 30.0 |> PlotOption
EuroPut 70.0 |> PlotOption










let BottomStraddle exercisePrice actualPrice = 
  // buy put at exercise & buy call at exercise
  EuroCall exercisePrice actualPrice +
  EuroPut exercisePrice actualPrice


let Strangle lowPrice highPrice actualPrice = 
  // buy put at low & buy call at high
  EuroCall highPrice actualPrice +
  EuroPut lowPrice actualPrice


let BullSpread lowPrice highPrice actualPrice = 
  // buy call at low & sell call at high
  (EuroCall lowPrice actualPrice) -
  (EuroCall highPrice actualPrice)

    
EuroCall 30.0 |> PlotOption
EuroPut 70.0 |> PlotOption

BottomStraddle 40.0 |> PlotOption
Strangle 30.0 60.0 |> PlotOption
BullSpread 30.0 60.0 |> PlotOption


// -------------------------------------------------------

let ButterflySpread lowPrice highPrice actualPrice = 
  (EuroCall lowPrice actualPrice) +
  (EuroCall highPrice actualPrice) -
  2.0 * (EuroCall ((lowPrice + highPrice) / 2.0) actualPrice)


Chart.Rows
 [ Chart.Columns
    [ PlotOption(BottomStraddle 30.0).AndTitle("Bottom straddle") 
      PlotOption(ButterflySpread 20.0 80.0).AndTitle("Butterfly spread") ]
   Chart.Columns
    [ PlotOption(BullSpread 20.0 80.0).AndTitle("Bull spread") 
      PlotOption(Strangle 20.0 80.0).AndTitle("Strangle") ] ]

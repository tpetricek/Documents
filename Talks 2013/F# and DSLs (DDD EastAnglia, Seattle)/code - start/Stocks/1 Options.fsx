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

// DEMO
//   European call option: actualPrice - exercisePrice or zero
//   European put option:  exercisePrice - actualPrice or zero


// DEMO
//   EuroCall 30.0 |> PlotOption
//   EuroPut 70.0 |> PlotOption

// DEMO
//   BottomStraddle (Call at price + Put at price)
//   Strangle (Call at high + Put at low)
//   BullSpread (Call - Put)
//   ButterflySpread 

// DEMO    
//   BottomStraddle 40.0 |> PlotOption
//   Strangle 30.0 60.0 |> PlotOption
//   BullSpread 30.0 60.0 |> PlotOption

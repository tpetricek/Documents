open System
open System.Drawing

#r "System.Windows.Forms.DataVisualization.dll"
#r "bin\\MSDN.FSharpChart.dll"
#load "bin\\FSharpChart.fsx"
open MSDN.FSharp.Charting

#load "1_Data.fsx"
open StockData

let aapl = loadStockPrices "aapl" |> Array.rev
let msft = loadStockPrices "msft" |> Array.rev

let msft100 = loadStockPrices "msft" |> Seq.take 100 |> Array.ofSeq |> Array.rev
let aapl100 = loadStockPrices "aapl" |> Seq.take 100 |> Array.ofSeq |> Array.rev

// ------------------------------------------------------------------
// Processing data using F#

// Show average price using a line chart
[ for dt, (o,h,l,c) in aapl -> o ] 
|> FSharpChart.Line


// Creates a financial 'candlestick' chart that shows all four values
[ for dt, (o,h,l,c) in msft100 -> h, l, o, c ]
|> FSharpChart.Candlestick
|> FSharpChart.WithArea.AxisY(Minimum = 20.0, Maximum = 30.0)


// Visualize the price range using an area chart
[ for dt, (o, h, l, c) in msft100 -> l, h ]
|> FSharpChart.Range
|> FSharpChart.WithArea.AxisY(Minimum = 20.0, Maximum = 30.0)
|> FSharpChart.WithSeries.Style
     ( Color = Color.FromArgb(32, 135, 206, 250), 
       BorderColor = Color.SkyBlue, BorderWidth = 1)


// Combine multiple series in a single chart
FSharpChart.Combine
  [ FSharpChart.Line [ for dt, (o,h,l,c) in aapl100 -> o ] 
    FSharpChart.Line [ for dt, (o,h,l,c) in msft100 -> o ] ]


// ==================================================================
// TASK #6
// Create chart that shows values with 5 day average.
// This can be done using Seq.windowed

// (...)

// ==================================================================
// TASK #7
// Create chart that shows prices together with 
// standard deviation (over 5 day window) range

// (...)
#r "bin\\FSharp.AsyncExtensions.dll"
#load "ClassifierDSL.fs"
#load "FSharpChart.fsx"
#load "YahooData.fsx"

open System
open YahooData
open ClassifierDSL
open MSDN.FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting
open FSharp.Control 
open System.Windows.Forms
open System.Drawing

let form = new Form(Visible = true, TopMost = true)
let classifiers = ResizeArray<_>()

let dashGrid = 
    Grid( LineColor = Color.Gainsboro, 
          LineDashStyle = ChartDashStyle.Dash )

let refresh data = 
  form.Invoke(Action(fun () ->
    for (ctl:Control), classif in classifiers do
      let matched = run classif data
      printfn "%s %A" ((ctl :?> Label).Text) matched
      let clr = if matched then Color.Red else Color.White
      ctl.BackColor <- clr )) |> ignore

let add name classifier = 
  let ctl = new Label(Width = 100, Height = 30, Top = classifiers.Count * 40)
  ctl.Text <- name
  classifiers.Add(ctl :> Control, classifier)
  form.Controls.Add(ctl)

let aapl = YahooData.getPricesWindowed 30 500 (DateTime(2010, 1, 1)) "msft"

aapl 
|> Observable.windowed 30
|> Observable.add refresh

both minimum maximum |> map (fun (l, h) -> h - l)
bothAnd (inRange 20.0 30.0) declining

FSharpChart.Line(aapl, MaxPoints = 30)
|> FSharpChart.WithArea.AxisY(Minimum = 100.0, Maximum = 400.0, MajorGrid = dashGrid)
|> FSharpChart.WithArea.AxisX(LabelStyle = new LabelStyle(Enabled = false), MajorGrid = dashGrid)
|> FSharpChart.WithMargin(0.0f, 7.0f, 2.0f, 2.0f)
|> FSharpChart.WithTitle("Stocks (MSFT)", Font = new Font("Calibri",13.0f))
|> FSharpChart.Create

let downUp = (Price.sequenceAnd (Price.regression Price.declining) (Price.regression Price.rising)) 
let upDown = (Price.sequenceAnd (Price.regression Price.rising) (Price.regression Price.declining)) 
add "rising" (Price.regression Price.rising)
add "declining" (Price.regression Price.declining)
add "Down & Up" downUp
add "Up & Down" upDown
add "W pattern" (Price.sequenceAnd downUp downUp)




(*
run (OverRegression rising) msft

run (SequenceAnd (OverRegression declining) (OverRegression rising)) msft



// Is the price always rising or declining?
run rising data
run declining data

// Is the price rising or declining over a linear regression?
run (OverRegression rising) data
run (OverRegression declining) data

// Find 5 subsequent days where the price is rising 
// and then declining for the same period
let pat1 = SequenceAnd rising declining
windowed 5 pat1 data |> showDates

// Find 2 subsequent days where the price is between 
// 1.0 and 2.0 and is declining at the same time
let pat2 = Both declining (InRange 1.0 2.0)
windowed 2 pat2 data |> showDates

// Find 7 days where the price is mostly rising over first 4 and then 
// mostly declining over the last 4 (middle day is used twice). This exists,
// but not if we were using the actual prices - just using regression.
let pat3 = SequenceAnd (OverRegression rising) (OverRegression declining)
windowed 7 pat3 data |> showDates

*)
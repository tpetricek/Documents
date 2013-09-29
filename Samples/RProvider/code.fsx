// --------------------------------------------------------
// RProvider, F# Data and F# Charting demo
// Based on: http://www.tryfsharp.org/Learn/financial-computing#simulating-and-analyzing
// --------------------------------------------------------

#I "lib"
#load "FSharp.Charting.fsx"
#r @"RDotNet.dll"
#r @"RProvider.dll"
#r @"FSharp.Data.dll"
open System
open FSharp.Data
open FSharp.Charting

// --------------------------------------------------------
// Getting data with F# data
// --------------------------------------------------------

type StockData = CsvProvider<"data/MSFT.csv", Schema="date,float,float,float,float,float,float">
let allMsftData = StockData.Load("data/MSFT.csv")

let msftData = allMsftData.Data |> List.ofSeq |> List.rev

let getPrices (data:seq<StockData.Row>) days =
  let maxDate = data |> Seq.map (fun r -> r.Date) |> Seq.max
  [ for d in data do
      if d.Date > maxDate.AddDays(-(float days)) then
        yield d.Date, float d.Close ] 
  |> List.sortBy fst

let msft = getPrices msftData 100
let msftDates, msftValues = getPrices msftData 100 |> List.unzip

// --------------------------------------------------------
// R provider
// --------------------------------------------------------
 
open RDotNet
open RProvider
open RProvider.graphics
open RProvider.``base``
open RProvider.stats      // rnorm
open RProvider.TTR        // volatility - install.packages("TTR") 

// Basic plots
R.plot [ 1;2;3;4;6;9;20;100 ]
R.plot(msftDates, msftValues, [| "l" |])


// Save chart to PDF
open RProvider.grDevices
R.pdf("D:\\temp\\test2.pdf")
namedParams 
  ["x", box msftDates; "y", box msftValues; "type", box "l";
   "xlab", box "Numbers"; "ylab", box "Also numbers" ]
|> R.plot
R.dev_off()


// Writing F# helper functions
let line data = 
  R.plot(data |> Seq.mapi (fun i _ -> i), data, "l")
let addline (data:seq<float>) = 
  R.lines(data)
line(msftValues)


// Infinite sequence of normally distributed values
let rec normal (mean:float) (sd:float) = seq {
  yield! R.rnorm(100, mean, sd).AsNumeric()
  yield! normal mean sd }
// Creates the expected histogram :-)
R.hist(normal 0.0 1.0 |> Seq.take 1000, 20)


// Sampling from a sequence of values
let sample (values:seq<_>) = 
  let en = values.GetEnumerator()
  fun () -> en.MoveNext() |> ignore; en.Current

// Generate 10 random walks (maybe nicer with Seq.unfold)
let ndist = normal 0.0 1.0 
let randomWalk initial = 
  let dist = ndist |> sample
  let rec loop value = seq { 
    yield value 
    let p = dist()
    yield! loop (value + p) }
  loop initial 
Chart.Combine
  [ for i in 0 .. 10 ->
      Chart.Line(randomWalk 0.0 |> Seq.take 100) ]


// --------------------------------------------------------
// Generating asset price movement 
// --------------------------------------------------------

// Generating prices using geometric browninan motion
let randomPrice drift volatility dt initial dist = 
  let driftExp = (drift - 0.5 * pown volatility 2) * dt
  let randExp = volatility * (sqrt dt)
  let rec loop price = seq {
    yield price
    let price = price * exp (driftExp + randExp * dist()) 
    yield! loop price }
  loop initial

let ndist' = normal 0.0 1.0 |> Seq.cache
let dist1 = ndist' |> sample
let dist2 = ndist' |> sample

// Generate two movements with different volatility, but
// same random numbers and same drift
let data1 = randomPrice 0.10 0.30 0.005 5.0 dist2 |> Seq.take 500
let data2 = randomPrice 0.10 0.10 0.005 5.0 dist1 |> Seq.take 500

// This looks a bit ugly
line data1
addline data2

// F# Chart variant looks nicer
Chart.Combine
  [ Chart.Line(data1) 
    Chart.Line(data2) ]

// --------------------------------------------------------
// Calculating historical volatility using R
// --------------------------------------------------------

// Create R data frame with OHLC data for MSFT
let df =
  [ "Open",  box [| for r in msftData -> r.Open |]
    "High",  box [| for r in msftData -> r.High |]
    "Low",   box [| for r in msftData -> r.Low |]
    "Close", box [| for r in msftData -> r.Close |] ]
  |> namedParams
  |> R.data_frame

// Use volatility function from R and get vector of floats back
// (not entirely sure what this does!?)
let p = namedParams ["OHLC", box df; "calc", box "close"]
let vols = R.volatility(p).AsNumeric() |> List.ofSeq

Chart.Rows
  [ vols |> Chart.Line
    msftValues |> Chart.Line ]

// This calculates sequence of logarithm of ratios
let divs = 
  msftValues 
  |> Seq.pairwise
  |> Seq.map (fun (prev, next) -> log (next / prev))

// Tau is the 1.0/number of trading days
let tau = 1.0 / 252.0
let var = R.var(divs).AsNumeric() |> Seq.head
let mean = R.mean(divs).AsNumeric() |> Seq.head

// Calculate the right values to get chart that "simulates" MSFT price
let volatilityMsft = sqrt (var / tau)
let driftMsft = (mean / tau) + (pown volatilityMsft 2) / 2.0
let first = (msftValues |> Seq.head)
let dist = normal 0.0 1.0 |> sample

// Generate multiple simulated price movements & compare with actual data
[ for i in 1 .. 3 ->
    let msftFake = 
      randomPrice driftMsft volatilityMsft tau first dist 
      |> Seq.take msftValues.Length
    Chart.Combine(
       [ Chart.Line(msftValues, Name=sprintf "Real #%d" i)
         Chart.Line(msftFake, Name=sprintf "Fake #%d" i) ]).WithYAxis(Min=22.0, Max=35.0) ]
|> Chart.Rows

// --------------------------------------------------------
// ggplot2
// --------------------------------------------------------

open RProvider.graphics
open RProvider.ggplot2

// Nice plot with smooth interpolation using ggplot2
R.qplot
  ( x = msftDates, 
    y = msftValues, 
    geom=[| "point"; "smooth" |])
  |> R.print

// Histogram showing how common various price changes are
R.qplot
  ( x = [| for r in msftData -> r.Open - r.Close |],
    geom = [| "histogram" |] )
  |> R.print

// Store data frame with all data in a R variable
let dfAll =
  [ "Date",  box [| for r in msftData -> r.Date |]
    "Open",  box [| for r in msftData -> r.Open |]
    "High",  box [| for r in msftData -> r.High |]
    "Low",   box [| for r in msftData -> r.Low |]
    "Close", box [| for r in msftData -> r.Close |] ]
  |> namedParams |> R.data_frame
R.assign("ohlc", dfAll)

// Get closing prices by evaluating R expression
R.eval(R.parse(namedParams["text", "ohlc$Close"])).AsNumeric()

// Helper that evaluates R expression
let eval (text:string) =
  R.eval(R.parse(namedParams ["text", text ]))

// Do some more calculations
R.assign("ohlc", dfAll)
eval("mean(ohlc$Open)").AsNumeric() |> Seq.head
eval("mean(ohlc$Close)").AsNumeric() |> Seq.head

// Use ggplot2 and do some charting by passing string to the eval function
eval("library(\"ggplot2\")")

eval("""
  print(
    ggplot(ohlc, aes(x=Date, y=Open)) + 
    geom_line() + 
    geom_smooth()
  )
  """)

eval("""
  print(
    ggplot(ohlc, aes(x=Date)) + 
    geom_line(aes(y=High, color="red")) + 
    geom_line(aes(y=Low, color="Blue")) 
  )
  """)

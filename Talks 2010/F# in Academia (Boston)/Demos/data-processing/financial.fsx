open System
open System.Net

// ----------------------------------------------------------------------------
// Specifying the structure of data
// ----------------------------------------------------------------------------

/// Declare unit of measure representing dolars
[<Measure>] type USD

/// Function that converts a string to dolarss
let inline usd s = 1.0<USD> * float s

/// Represents stock price range at the specified date
type StockPrice = 
  { Date : DateTime
    High : float<USD>
    Low : float<USD>
    Open : float<USD>
    Close : float<USD> }

/// History for a specified stock
type StockHistory = 
  { History : seq<StockPrice>
    Name : string }
    

// ----------------------------------------------------------------------------
// Downloading stock data from the Yahoo web site
// ----------------------------------------------------------------------------

/// Download price history for the specified stock (from the internet)
let download (stock:string) = async {
   let url = "http://ichart.finance.yahoo.com/table.csv?s=" + stock
   let wc = new WebClient()
   // Asynchronous operation without blocking threads!
   let! data = wc.AsyncDownloadString(Uri url)
   return data }

/// (Alternative version)
/// Download price history for the secified stock (from a file)
let downloadLocal (stock:string) = async {
  let path = sprintf "/home/tomas/Demos/%s.csv" (stock.ToLower())
  return System.IO.File.ReadAllText(path) }


/// Parse lines of the downloaded CSV file into StockPrice records
let parseLines (dataLines:seq<string>) =
  [| for line in dataLines do
       let infos = line.Split(',')
       yield { Date = DateTime.Parse(infos.[0])
               Open = infos.[1] |> usd
               High = infos.[2] |> usd
               Low = infos.[3] |> usd
               Close = infos.[4] |> usd } |]


/// Download data about a stock and create 'StockHistory'
let getStockPrices stock = async {
  let! data = download(stock)
  let dataLines = 
    data.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries)
    |> Seq.skip 1
  return { Name = stock; History = parseLines dataLines } }


// Download information about some specified stocks...
let allStocks =
  [ "YHOO"; "GOOG"; "MSFT" ]
  |> List.map getStockPrices 
  |> Async.Parallel
  |> Async.RunSynchronously


// ----------------------------------------------------------------------------
// Calculating median and standard deviation
// ----------------------------------------------------------------------------

for stock in allStocks do
  Console.WriteLine("*** {0} ***", stock.Name)

  // Calculate & print the average value  
  let avg = stock.History |> Seq.averageBy (fun s -> s.Close)
  Console.WriteLine("Average: ${0:0.00}", avg)

  // Calculate variance 
  let tmp = stock.History |> Seq.sumBy (fun v -> 
    (v.Close - avg) * (v.Close - avg))
  let variance = tmp / float ((stock.History |> Seq.length) - 1) 
  Console.WriteLine("Variance: {0:0.00}", variance)

  // DEMO: Calculate expected range
  //   The following causes compile time error (units don't match!)
  //   let min, max = avg - variance, avg + variance

  let sdv = sqrt variance
  let min, max = avg - sdv, avg + sdv
  Console.WriteLine("Range: {0:0.00} - {1:0.00}", min, max)


// ----------------------------------------------------------------------------
// Plotting stock data using gnuplot
// ----------------------------------------------------------------------------

#load "gnuplot.fs"

open FSharp.GnuPlot
open System.Drawing

let gp = new GnuPlot()

// Draw a simple function
gp.Plot("sin(x)")

// Draw stock history using line chart
Series.Lines 
  [ for s in allStocks.[1].History -> float s.Close ]
  |> gp.Plot

// Combine history of all stocks for the last year  
let getYearCloseSeries data = 
  seq { for s in data -> float s.Close }
    |> Seq.take 365 |> List.ofSeq |> List.rev

// Plot history of all stocks  
[ for info in allStocks ->
    Series.Lines
      ( title = info.Name, weight = 2, 
        data = getYearCloseSeries info.History) ]
  |> gp.Plot


// Set logarithmic scale so that we can compare prices
gp.SendCommand("set logscale y")
gp.SendCommand("unset logscale y")


// Display finance bars with opening/minimal/maximal/closing values
let getFinanceMonthSeries data = 
  seq { for s in data -> 
          float s.Open, float s.Low, float s.High, float s.Close }
    |> Seq.take 30 |> List.ofSeq |> List.rev

Series.FinacneBars
  ( title = "GOOG", weight = 1, 
    data = getFinanceMonthSeries allStocks.[1].History )
|> gp.Plots
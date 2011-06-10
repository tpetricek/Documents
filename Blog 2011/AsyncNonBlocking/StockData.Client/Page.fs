namespace StockData.Client

open System
open System.Net
open System.Windows
open System.Threading
open System.Windows.Controls
open System.Windows.Shapes
open System.Windows.Media

open FSharp.Control.AsyncExtensions

type AppControl() =
  inherit UserControl(Width = 800.0, Height = 600.0)

  // ------------------------------------------------------------------------------------
  // Functions & values that are not asynchronous

  /// Selected stocks with associated color
  let stocks = 
    [ Color.FromArgb(255uy, 0uy, 30uy, 190uy), "MSFT"; 
      Color.FromArgb(255uy, 30uy, 200uy, 60uy), "GOOG"; 
      Color.FromArgb(255uy, 190uy, 0uy, 0uy), "YHOO" ]

  // Create user interface
  let fill = SolidColorBrush(Color.FromArgb(255uy, 255uy, 255uy, 255uy))
  let canv = new Canvas(Background = fill)
  let holder = new Canvas()
  do canv.Children.Add(holder)

  let lbl = new TextBlock(Width = 40.0, Height = 20.0, Text = "Hi..")
  do 
    lbl.SetValue(Canvas.LeftProperty, 220.0)
    lbl.SetValue(Canvas.TopProperty, 15.0)
    canv.Children.Add(lbl) 

  let buttons = stocks |> List.mapi (fun i stock ->
    let btn = new Button(Width = 40.0, Height = 20.0, Content = snd stock)
    btn.SetValue(Canvas.LeftProperty, 50.0 + (float i) * 50.0)
    btn.SetValue(Canvas.TopProperty, 10.0)
    canv.Children.Add(btn) 
    btn )

  /// Create path from the specified collection of points
  let createPath data =
    let lineSegments = new PathSegmentCollection()
    for point in data do 
        lineSegments.Add(LineSegment(Point = point)) 
    let figure = new PathFigure(StartPoint = Seq.head data, IsClosed = false, Segments = lineSegments)
    let geometry = new PathGeometry()
    geometry.Figures.Add(figure)
    new Path(Data = geometry) 

  /// Create chart for the specified data (with the specified width/height)
  let createChart width height clr values = 
    let fill = SolidColorBrush(Color.FromArgb(255uy, 255uy, 255uy, 255uy))
    let area = new Canvas(Background = fill, Width = width, Height = height)
    let max, min = Seq.max values, Seq.min values

    let path = values |> Seq.mapi (fun i v -> Point(float i, (v - min) / (max - min) * height))  |> createPath
    let fill = SolidColorBrush(clr)
    path.Stroke <- fill
    path.StrokeThickness <- 2.0
    area.Children.Add(path) 
    area

  /// URL of the service 
  let root = System.Windows.Browser.HtmlPage.Document.DocumentUri.ToString() + "Data/"
  do System.Diagnostics.Debug.WriteLine(root)

  let phase = ref "Started"
  let setPhase s = phase := s

  // ------------------------------------------------------------------------------------
  // Utility asynchronous functions

  /// Parse downloaded data set. This is a CPU-expensive 
  /// computation that should not block the user-interface
  let extractStockPrices (data:string) count = async {
    let dataLines = 
      data.Split([| '\n' |], StringSplitOptions.RemoveEmptyEntries) 
    let data = 
      [| for line in dataLines |> Seq.skip 1 do
           let infos = line.Split(',')
           yield float infos.[1] |]
      |> Seq.take count |> Array.ofSeq |> Array.rev 
    // Additional CPU-bound processing
    Thread.Sleep(5000)
    return data }

  /// Create & display chart. The function needs to 
  /// access user interface and should run on GUI thread
  let displayChart color prices = async {
    let chart = createChart 600.0 200.0 color prices
    holder.Children.Clear()
    chart.SetValue(Canvas.LeftProperty, 100.0)
    chart.SetValue(Canvas.TopProperty, 100.0)
    holder.Children.Add(chart) }

  // ------------------------------------------------------------------------------------
  // Main function of the applicataion

  let main = async {
    // Create aggregated click event
    let stockClicked = 
      [ for stock, btn in Seq.zip stocks buttons ->
          btn.Click |> Observable.map (fun _ -> stock) ]
      |> List.reduce Observable.merge
    
    // The main application loop
    while true do 
      // Wait until the user selects stock (GUI operation)
      setPhase "Waiting"
      let! color, stock = Async.AwaitObservable(stockClicked)

      // Download data for stock (non-blocking operation)
      setPhase "Downloading"
      let wc = new WebClient()
      let! data = wc.AsyncDownloadString(Uri(root + stock))    

      // Process data (CPU-bound operation)
      setPhase "Processing"
      let ctx = SynchronizationContext.Current
      do! Async.SwitchToThreadPool()
      let! prices = extractStockPrices data 500
      do! Async.SwitchToContext(ctx)

      // Create & display the chart (GUI operation)
      do! displayChart color prices }

  do
    base.Content <- canv

    // Simple async workflow to demonstrate progress
    async { 
      while true do 
        let dots = ref "."
        lbl.Text <- !phase + !dots
        for i in 0 .. 5 do 
          do! Async.Sleep(500)
          dots := !dots + "."
          lbl.Text <- !phase + !dots }
    |> Async.StartImmediate

    main |> Async.StartImmediate

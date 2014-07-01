open System
open System.Net
open System.Windows
open System.Threading

// ------------------------------------------------------------------
// Domain-specific language for creating classifiers

/// Represents a classifier that produces a value 'T
type Classifier<'T> = PT of ((DateTime * float)[] -> 'T)


/// Simple classifiers that extract value or check property
module Price =

  // ----------------------------------------------------------------
  // Basic functions for composition

  /// Runs a classifier and transforms the result using a specified function
  let map g (PT f) = PT (f >> g)

  /// Classifier that alwasy succeeds & returns the specified value
  let unit v = PT (fun _ -> v)
  /// Classifier that applies two classifiers in sequence
  let bind f (PT g) = PT (fun values -> 
    let (PT r) = f (g values) in r values)

  /// Simple classifier that always returns true
  let always = unit true

  /// Creates a classifier that combines the result of two classifiers using a tuple
  let both (PT f) (PT g) = PT (fun values -> f values, g values)

  /// Checks two properties of subsequent parts of the input
  let sequence (PT f1) (PT f2) = PT (fun input ->
    let length = input.Length
    let input1 = input.[0 .. length/2 - (if length%2=0 then 1 else 0)]
    let input2 = input.[length/2 .. length-1]
    (f1 input1, f2 input2))

  /// Gets the minimum over the whole range
  let reduce f = PT (fun input ->
    input |> Seq.map snd |> Seq.reduce f)

  // ----------------------------------------------------------------
  // Primitive classifiers

  /// Checks whether the price is rising over the whole checked range
  let rising = PT (fun input ->
    input |> Seq.pairwise |> Seq.forall (fun ((_, a), (_, b)) -> b >= a))

  /// Checks whether the price is declining over the whole checked range
  let declining = PT (fun input ->
    input |> Seq.pairwise |> Seq.forall (fun ((_, a), (_, b)) -> b <= a))

  /// Gets the minimum over the whole range
  let minimum = reduce min |> map (fun v -> Math.Round(v, 2))

  /// Gets the maximum over the whole range
  let maximum = reduce max |> map (fun v -> Math.Round(v, 2))

  /// Gets the maximum over the whole range
  let average = PT (fun input ->
    Math.Round(input |> Seq.map snd |> Seq.average, 2) )

  /// Checks that the price is at least the specified value in the whole range
  let atLeast min = PT (Seq.forall (fun (_, v) -> v >= min))

  /// Checks that the price is at most the specified value in the whole range
  let atMost max = PT (Seq.forall (fun (_, v) -> v <= max))

  // ----------------------------------------------------------------
  // Advanced combinators

  /// Checks that two properties hold for subsequent parts of the input
  let sequenceAnd a b = sequence a b |> map (fun (a, b) -> a && b)
  /// Checks that two properties hold for the same input
  let bothAnd a b = both a b |> map (fun (a, b) -> a && b)
  /// Checks that one of the properties holds for subsequent parts of the input
  let sequenceOr a b = sequence a b |> map (fun (a, b) -> a || b)
  /// Checks that one of the properties holds for the same input
  let bothOr a b = both a b |> map (fun (a, b) -> a || b)

  /// Checks that the price is withing a specified range over the whole input
  let inRange min max = bothAnd (atLeast min) (atMost max)

  /// Checks that the property holds over an approximation 
  /// obtained using linear regression
  let regression (PT f) = PT (fun values ->
    // TODO: Use date time in case it is not linear
    let xavg = float (values.Length - 1) / 2.0
    let yavg = Seq.averageBy snd values
    let sums = values |> Seq.mapi (fun x (_, v) -> 
      (float x - xavg) * (v - yavg), pown (float x - xavg) 2)
    let v1 = Seq.sumBy fst sums
    let v2 = Seq.sumBy snd sums
    let a = v1 / v2
    let b = yavg - a * xavg 
    values |> Array.mapi (fun x (dt, _) -> (dt, a * (float x) + b)) |> f)

/// Computation expression builder for building classifiers
type ClassifierBuilder() = 
  member x.Return(v) = Price.unit v
  member x.Bind(c, f) = Price.bind f c

/// Instance of computation expression builder for classifiers
let classify = ClassifierBuilder()

/// Does the property hold over the entire data set?
let run (PT f) (data:(DateTime * float)[]) = 
  f data


// ------------------------------------------------------------------
// Downloading stock prices from Yahoo


/// Asynchronously downloads stock prices from Yahoo
let downloadPrices from stock = async {
  // Download price from Yahoo
  let wc = new WebClient()
  let url = "http://ichart.finance.yahoo.com/table.csv?s=" + stock
  let! html = wc.AsyncDownloadString(Uri(url)) 
  let lines = html.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
  let lines = lines |> Seq.skip 1 |> Array.ofSeq |> Array.rev

  // Return sequence that reads the prices
  let data = seq { 
    while true do
      for line in lines do
        let infos = (line:string).Split(',')
        let dt = DateTime.Parse(infos.[0])
        let op = float infos.[1] 
        if dt > from then yield dt, op } 

  return data }

// ------------------------------------------------------------------
// Visualizing stock prices using FSharpChart

#load "lib\\FSharpChart.fsx"
open MSDN.FSharp.Charting
open ChartData

open System.Windows.Forms
open System.Drawing

type ClassifierWindow() = 
  inherit Form(Visible=true, Width=800, Height=500)

  /// List of update functions to be called from GUI
  let updates = ResizeArray<((DateTime * float)[] -> unit) * (unit -> unit)>()
  let cleanup = ref ignore

  // Current cancellation token
  let tok = ref <| new CancellationTokenSource()

  let grid = System.Windows.Forms.DataVisualization.Charting.Grid(LineColor=System.Drawing.Color.DimGray)
  let ds = new OneValue()
  let ch = 
    FSharpChart.Line [ 0.0 .. 100.0 ]
    |> FSharpChart.WithArea.AxisY(MajorGrid=grid)
    |> FSharpChart.WithArea.AxisX(MajorGrid=grid)

  let chart = new ChartControl(ch, Dock = DockStyle.Fill)
  let chartArea = (chart.Controls.[0] :?> System.Windows.Forms.DataVisualization.Charting.Chart).ChartAreas.[0]
  do chartArea.BackColor <- Color.Black
  do chartArea.AxisX.TitleForeColor <- Color.White
  do chartArea.AxisY.TitleForeColor <- Color.White
  do chart.BackColor <- Color.Black
  do base.BackColor <- Color.Black
  do base.ForeColor <- Color.White
  do ((chart.Controls.[1] :?> System.Windows.Forms.PropertyGrid).SelectedObject :?> System.Windows.Forms.DataVisualization.Charting.Chart).BackColor  <- System.Drawing.Color.Black
  do chartArea.AxisX.LineColor <- System.Drawing.Color.White
  do chartArea.AxisY.LineColor <- System.Drawing.Color.White
  do chartArea.AxisX.LabelStyle.ForeColor <- System.Drawing.Color.White
  do chartArea.AxisY.LabelStyle.ForeColor <- System.Drawing.Color.White

  let split = new SplitContainer(Dock = DockStyle.Fill)
  do 
    base.Controls.Add(split)
    split.SplitterDistance <- 520
    split.Panel1.Controls.Add(chart)
  do ds.BindSeries(chart.ChartSeries)

  /// Add classifier to list & create GUI
  let addBoolClassifier name (cls:Classifier<bool>) = 
    let cont = new Panel(Anchor = (AnchorStyles.Left ||| AnchorStyles.Right ||| AnchorStyles.Top), Height=50, Width=split.Panel2.Width, Top=split.Panel2.Controls.Count*50)
    let box = new Panel(Width = 60, Height = 30, Top = 10, Left = 10, BackColor=Color.LightGray)
    cont.Controls.Add(box)
    split.Panel2.Controls.Add(cont)
    let block = new Label(Text = name, Height=50, Left=80, Width=split.Panel2.Width-60,Anchor = (AnchorStyles.Left ||| AnchorStyles.Right), TextAlign = ContentAlignment.MiddleLeft)
    block.Font <- new System.Drawing.Font("Calibri", 15.0f)
    cont.Controls.Add(block)

    let update data =
      box.BackColor <- if run cls data then Color.YellowGreen else Color.DimGray
    let clear () = 
      split.Panel2.Controls.Remove(cont)
    updates.Add( (update, clear) ) 

  /// Add classifier to list & create GUI
  let addFloatClassifier name (cls:Classifier<float>) = 
    let cont = new Panel(Anchor = (AnchorStyles.Left ||| AnchorStyles.Right ||| AnchorStyles.Top), Height=50, Width=split.Panel2.Width, Top=split.Panel2.Controls.Count*50)
    let box = new Label(Width = 60, Height = 30, Top = 10, Left = 10, TextAlign = ContentAlignment.MiddleCenter)
    cont.Controls.Add(box)
    split.Panel2.Controls.Add(cont)
    let block = new Label(Text = name, Height=50, Left=80, Width=split.Panel2.Width-60,Anchor = (AnchorStyles.Left ||| AnchorStyles.Right), TextAlign = ContentAlignment.MiddleLeft)
    block.Font <- new System.Drawing.Font("Calibri", 15.0f)
    box.Font <- new System.Drawing.Font("Calibri", 15.0f)
    cont.Controls.Add(block)

    let update data =
      box.Text <- string (run cls data)
    let clear () = 
      split.Panel2.Controls.Remove(cont)
    updates.Add( (update, clear) ) 

  /// Main loop
  let mainLoop stock = async {
    let! prices = downloadPrices (DateTime(2009, 1, 1)) stock
    let blocks = prices |> Seq.windowed 30
    let en = blocks.GetEnumerator()

    while en.MoveNext() do
      do! Async.Sleep(125)
      for fn, _ in updates do fn en.Current

      let lo = Seq.min (Seq.map snd en.Current)
      let hi = Seq.max (Seq.map snd en.Current)
      let diff = (hi - lo) / 6.0
      chartArea.AxisY.Maximum <- ceil (hi + diff)
      chartArea.AxisY.Minimum <- floor (lo - diff)
      ds.SetData(Array.map snd en.Current)  } 

  member x.Run(stock) =
    let cts = new CancellationTokenSource()
    x.Closing.Add(fun _ -> cts.Cancel())
    tok := cts
    Async.StartImmediate(mainLoop stock, cts.Token)

  member x.Add(name, cls) =
    addBoolClassifier name cls

  member x.Add(name, cls) =
    addFloatClassifier name cls

  member x.Clear() = 
    for _, clean in updates do clean ()
    updates.Clear() 

  member x.Stop() = 
    (!tok).Cancel()

  member x.Chart = chart
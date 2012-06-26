open System
open System.Net
open System.Windows
open System.Windows.Shapes
open System.Windows.Media
open System.Windows.Controls
open System.Threading
open Microsoft.TryFSharp

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

  let unit v = PT (fun _ -> v)
  let bind f (PT g) = PT (fun values -> let (PT r) = f (g values) in r values)

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

type ClassifierBuilder() = 
  member x.Return(v) = Price.unit v
  member x.Bind(c, f) = Price.bind f c

let classify = ClassifierBuilder()

/// Does the property hold over the entire data set?
let run (PT f) (data:(DateTime * float)[]) = 
  f data


// ------------------------------------------------------------------
// Downloading & visualizing stock prices from Yahoo


/// Asynchronously downloads stock prices from Yahoo
/// (uses a proxy to enable cross-domain downloads)
let downloadPrices from stock = async {
  // Download price from Yahoo
  let wc = new WebClient()
  let url = "http://ichart.finance.yahoo.com/table.csv?s=" + stock
  let proxy = "http://tomasp.net/tryjoinads/proxy.aspx?url=" + url
  let! html = wc.AsyncDownloadString(Uri(proxy)) 
  let lines = html.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)

  // Return sequence that reads the prices
  let data = seq { 
    for line in lines |> Seq.skip 1 do
      let infos = (line:string).Split(',')
      let dt = DateTime.Parse(infos.[0])
      let op = float infos.[1] 
      if dt > from then yield dt, op } 
  return data |> Array.ofSeq |> Array.rev |> Seq.ofArray }


/// Create a line geometry from a sequence of float values
let createGeometry width height data =
  let offsetX, offsetY = 20.0, 20.0
  let min = Seq.min data
  let max = Seq.max data
  let scale v = height - ((v - min) / (max - min) * height)
  let data = data |> Seq.map scale |> Seq.pairwise

  let geometry = new GeometryGroup()
  let step = width / float (Seq.length data)
  for i, (prev, next) in Seq.zip [ 0 .. Seq.length data ] data do
    let f = Point(offsetX + float i * step, prev + offsetY)
    let t = Point(offsetX + float (i + 1) * step, next + offsetY)
    let line = LineGeometry(StartPoint = f, EndPoint = t)
    geometry.Children.Add(line)

  geometry


/// Create line chart - returns a function that can be
/// used to set the data of the line chart
let createLineChart color width height =
  let path = new Path(Stroke = new SolidColorBrush(color), StrokeThickness = 2.0)
  App.Console.Canvas.Children.Add(path)
  let cleanup () = App.Console.Canvas.Children.Remove(path) |> ignore
  (fun data -> path.Data <- createGeometry width height data), cleanup


/// Runs an F# asynchronous workflow on the GUI thread in TryFSharp
let runUserInterface work = 
  let tok = new CancellationTokenSource()
  App.Dispatch (fun() -> 
    Async.StartImmediate(work, tok.Token)
    App.Console.CanvasPosition <- CanvasPosition.Right) |> ignore
  tok

type ClassifierWindow() = 

  /// List of update functions to be called from GUI
  let updates = ResizeArray<((DateTime * float)[] -> unit) * (unit -> unit)>()
  let cleanup = ref ignore

  let addControl ctl x y =
    Canvas.SetTop(ctl, y)
    Canvas.SetLeft(ctl, x)
    App.Console.Canvas.Children.Add(ctl)

  /// Add classifier to list & create GUI
  let addBoolClassifier name (cls:Classifier<bool>) = 
    App.Dispatch (fun() -> 
      let box = Rectangle(Width = 30.0, Height = 30.0)
      addControl box 30.0 (250.0 + float (updates.Count * 50))
      let block = TextBlock(FontSize = 20.0, Text = name, Width = 200.0)
      addControl block 100.0 (250.0 + float (updates.Count * 50))

      let update data =
        box.Fill <- new SolidColorBrush(if run cls data then Colors.Green else Colors.LightGray)
      let cleanup () = 
        App.Console.Canvas.Children.Remove(box) |> ignore
        App.Console.Canvas.Children.Remove(block) |> ignore

      updates.Add( (update, cleanup) ) ) |> ignore

  /// Add classifier to list & create GUI
  let addFloatClassifier name (cls:Classifier<float>) = 
    App.Dispatch (fun() -> 
      let box = TextBlock(FontSize = 20.0, Width = 30.0, Height = 30.0)
      addControl box 20.0 (250.0 + float (updates.Count * 50))
      let block = TextBlock(FontSize = 20.0, Text = name, Width = 200.0)
      addControl block 100.0 (250.0 + float (updates.Count * 50))

      let update data =
        box.Text <- string (run cls data)
      let cleanup () = 
        App.Console.Canvas.Children.Remove(box) |> ignore
        App.Console.Canvas.Children.Remove(block) |> ignore

      updates.Add( (update, cleanup) ) ) |> ignore

  /// Main loop
  let mainLoop stock = async {
    let! prices = downloadPrices (DateTime(2009, 1, 1)) stock
    let blocks = prices |> Seq.windowed 30
    let en = blocks.GetEnumerator()
    let update, removeLine = createLineChart Colors.Black 500.0 200.0
    cleanup := removeLine

    while en.MoveNext() do
      do! Async.Sleep(200)
      for fn, _ in updates do fn en.Current
      update (en.Current |> Seq.map snd)  } 

  // Current cancellation token
  let tok = ref <| new CancellationTokenSource()

  do App.Dispatch (fun() -> 
    App.Console.CanvasPosition <- CanvasPosition.Right
    App.Console.ClearCanvas()) |> ignore

  member x.Add(name, cls) =
    addBoolClassifier name cls

  member x.Add(name, cls) =
    addFloatClassifier name cls

  member x.Clear() = 
    App.Dispatch (fun () ->
      for _, clean in updates do clean ()
      updates.Clear() ) |> ignore

  member x.Run(stock) =
    tok := runUserInterface (mainLoop stock)

  member x.Stop() = 
    (!tok).Cancel()
    App.Dispatch(!cleanup) |> ignore

App.Dispatch (fun () -> 
  App.Console.ClearOutput()
  App.Console.LoadFromUrl("http://fssnip.net/raw/bL") )
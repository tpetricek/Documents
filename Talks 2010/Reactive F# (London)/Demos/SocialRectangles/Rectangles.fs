namespace SocialRectangles

open Utils
open System
open System.Net
open System.Windows 
open System.Windows.Media
open System.Windows.Controls
open System.Xml.Linq

type Rectangles() as this =
  inherit UserControl()
  // ----------------------------------------------------------------
  // Initialize application & find known XAML elements

  let uri = new System.Uri("/SocialRectangles;component/Rectangles.xaml", UriKind.Relative)
  do Application.LoadComponent(this, uri)
  let main : Canvas = this?Main
  let colorSelect : ColorSelector.Selector = this?ColorSelect

  // ----------------------------------------------------------------
  // Moves control to the specified location

  let moveControl(ctl:FrameworkElement, start:Point, finish:Point) =
    ctl.Width <- abs(finish.X - start.X)
    ctl.Height <- abs(finish.Y - start.Y)
    Canvas.SetLeft(ctl, min start.X finish.X)
    Canvas.SetTop(ctl, min start.Y finish.Y)

  // ----------------------------------------------------------------
  // Receiving rectangles from the server

  let xn s = XName.Get(s)
  let floatAttr (el:XElement) s = float(el.Attribute(xn s).Value)
  let byteAttr (el:XElement) s = byte(el.Attribute(xn s).Value)
  let int64Attr (el:XElement) s = int64(el.Attribute(xn s).Value)

  let rec loadRectangles(time) = async {
    // Send request to the server asynchronously
    let wc = new WebClient()

    let uri = sprintf "http://localhost:28493/Service/get.aspx?timestamp=%d" time
    let! res = Async.GuardedAwaitObservable wc.DownloadStringCompleted 
                (fun () -> wc.DownloadStringAsync(new Uri(uri)))

    // Parse the document we get back as the result
    let time = ref time
    let doc = XDocument.Parse(res.Result)
    for rc in doc.Descendants(xn "Rectangle") do
        // Parse information about single rectangle
        let start = new Point(floatAttr rc "Left", floatAttr rc "Top")
        let finish = new Point(floatAttr rc "Right", floatAttr rc "Bottom")
        let clr = Color.FromArgb(255uy, byteAttr rc "R", byteAttr rc "G", byteAttr rc "B")
        let stamp = int64Attr rc "Stamp"
        if stamp > !time then time := stamp
        // Create rectangle and add it to the view      
        let rc = new Canvas(Background = SolidColorBrush(clr))
        moveControl(rc, start, finish)
        main.Children.Add(rc) 

    // Wait some time and then try checking again
    do! Async.Sleep 1000
    do! loadRectangles(!time) }

  do loadRectangles(0L) |> Async.StartImmediate

  // ----------------------------------------------------------------
  // Sending rectangles to the server

  let formatXml = 
    sprintf "<Rectangle Left=\"%f\" Top=\"%f\" Right=\"%f\" Bottom=\"%f\" R=\"%d\" G=\"%d\" B=\"%d\" />"

  let storeRectangle(start:Point, finish:Point, color:Color) =
    let wc = new WebClient()
    let xml = formatXml start.X start.Y finish.X finish.Y color.R color.G color.B
    wc.UploadStringAsync(new Uri("http://localhost:28493/Service/store.aspx"), xml)

  // ----------------------------------------------------------------
  // Drawing of rectangles using async workflows

  let transparentGray = 
    SolidColorBrush(Color.FromArgb(128uy, 164uy, 164uy, 164uy))

  let rec waiting() = async {
    let! md = Async.AwaitObservable(main.MouseLeftButtonDown)
    let rc = new Canvas(Background = transparentGray)
    main.Children.Add(rc) 
    do! drawing(rc, md.GetPosition(main)) }

  and drawing(rc:Canvas, pos) = async {
    let! evt = Async.AwaitObservable(main.MouseLeftButtonUp, main.MouseMove)
    match evt with
    | Choice1Of2(up) -> 
        storeRectangle(pos, up.GetPosition(main), colorSelect.CurrentColor)
        main.Children.Remove(rc) |> ignore
        do! waiting() 
    | Choice2Of2(move) ->
        moveControl(rc, pos, move.GetPosition(main))
        do! drawing(rc, pos) }

  do
    waiting() |> Async.StartImmediate
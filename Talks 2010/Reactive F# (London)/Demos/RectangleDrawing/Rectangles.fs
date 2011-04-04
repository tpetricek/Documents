namespace RectangleDrawing

open Utils
open System
open System.Windows 
open System.Windows.Media
open System.Windows.Controls

type Rectangles() as this =
  inherit UserControl()
  // ----------------------------------------------------------------
  // Initialize application & find known XAML elements

  let uri = new System.Uri("/RectangleDrawing;component/Rectangles.xaml", UriKind.Relative)
  do Application.LoadComponent(this, uri)
  let main : Canvas = this?Main
  let colorSelect : ColorSelector.Selector = this?ColorSelect

  // ----------------------------------------------------------------
  // Moves control to the specified location

  let moveControl (ctl:FrameworkElement) (start:Point) (finish:Point) =
    ctl.Width <- abs(finish.X - start.X)
    ctl.Height <- abs(finish.Y - start.Y)
    Canvas.SetLeft(ctl, min start.X finish.X)
    Canvas.SetTop(ctl, min start.Y finish.Y)

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
        rc.Background <- SolidColorBrush(colorSelect.CurrentColor)
        do! waiting() 
    | Choice2Of2(move) ->
        moveControl rc pos (move.GetPosition(main))
        do! drawing(rc, pos) }

  do
    waiting() |> Async.StartImmediate


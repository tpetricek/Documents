namespace MouseMoves
open Utils

open System
open System.Windows
open System.Windows.Shapes 
open System.Windows.Media
open System.Windows.Controls

type Mouse() as this =
  inherit UserControl()
  let uri = new System.Uri("/MouseMoves;component/Mouse.xaml", UriKind.Relative)
  do Application.LoadComponent(this, uri)

  let moveBall(x, y) =
    let ball : Ellipse = this?Ball
    Canvas.SetLeft(ball, x)
    Canvas.SetTop(ball, y)

  do 
    this.MouseMove
    |> Event.map (fun me -> 
          let pos = me.GetPosition(this)
          pos.X - 25.0, pos.Y - 25.0 )
    |> Event.filter (fun (x, y) -> 
          y > 100.0 && y < 250.0 && x > 100.0 && x < 350.0 )
    |> Event.add moveBall

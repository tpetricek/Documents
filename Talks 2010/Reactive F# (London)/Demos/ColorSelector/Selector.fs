namespace ColorSelector

open Utils
open System
open System.Windows 
open System.Windows.Media
open System.Windows.Controls

type ColorUpdate = 
  | Red of byte
  | Green of byte
  | Blue of byte

type Selector() as this =
  inherit UserControl()
  let uri = new System.Uri("/ColorSelector;component/Selector.xaml", UriKind.Relative)
  do Application.LoadComponent(this, uri)

  let sliderR : Slider = this?SliderRed
  let sliderG : Slider = this?SliderGreen
  let sliderB : Slider = this?SliderBlue
  let box : Canvas = this?ColorBox

  let r = sliderR.ValueChanged |> Event.map (fun r -> Red(byte r.NewValue))
  let g = sliderG.ValueChanged |> Event.map (fun r -> Green(byte r.NewValue))
  let b = sliderB.ValueChanged |> Event.map (fun r -> Blue(byte r.NewValue))
      
  let colorChanged = 
    Event.merge (Event.merge r g) b
    |> Event.scan (fun (r, g, b) update ->
        match update with
        | Red nr -> (nr, g, b)
        | Green ng -> (r, ng, b)
        | Blue nb -> (r, g, nb) ) (0uy, 0uy, 0uy)
    |> Event.map (fun (r, g, b) -> 
        Color.FromArgb(255uy, r, g, b)) 

  do colorChanged.Add (fun br -> box.Background <- SolidColorBrush(br))

  [<CLIEvent>]
  member x.ColorChanged = colorChanged
  member x.CurrentColor = (box.Background :?> SolidColorBrush).Color
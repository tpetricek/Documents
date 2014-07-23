#I "."
#r "FSharp.Data.dll"
open FSharp.Data
open System.Drawing
open System.Windows.Forms

type TrafficLight = Red | Orange | Green

type TrafficForm() =
  let form = new Form(Visible=true, TopMost=true, Width=120, Height=360)
  let red = new Panel(Width=80, Height=80, Top=20, Left=20)
  let orange = new Panel(Width=80, Height=80, Top=120, Left=20)
  let green = new Panel(Width=80, Height=80, Top=220, Left=20)
  do form.Controls.AddRange([|red; orange; green|])

  member x.DisplayLight light = 
    red.BackColor <- Color.Gray
    orange.BackColor <- Color.Gray
    green.BackColor <- Color.Gray
    match light with
    | Red -> red.BackColor <- Color.Red
    | Orange -> orange.BackColor <- Color.Orange
    | Green -> green.BackColor <- Color.Green

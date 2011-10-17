namespace Checkout

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

type App() as app =
  inherit Application()

  let workflow () = 
    let root = new Grid()
    app.MainWindow.Content <- root

    // Show the specified page
    let showPage ctrl = 
      root.Children.Clear()
      root.Children.Add(ctrl) |> ignore

    let check = new CheckoutLine()
    showPage(check.Control) 

  do 
    app.Startup.Add(fun _ ->
      workflow ())

// ------------------------------------------------------------------
  
module Main =
  [<STAThread>] 
  do 
    let win = new Window(Width = 815.0, Height = 625.0)
    (new App()).Run(win) |> ignore

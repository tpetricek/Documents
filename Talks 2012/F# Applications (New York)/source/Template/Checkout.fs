namespace Checkout

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

open Checkout.Domain

type App() as app =
  inherit Application()

  // Create the root control
  let root = new Grid()

  // Show the specified page
  let showPage ctrl = 
    root.Children.Clear()
    root.Children.Add(ctrl) |> ignore

  // -------------------------------------------------------

  let main () = async {
    app.MainWindow.Content <- root
    
    while true do
      let check = new CheckoutLine()
      showPage(check.Control)  
    
      let! sale = Async.AwaitEvent check.Completed 
      let fin = SaleOperations.CompleteSale sale

      let sum = new Summary(fin)
      showPage(sum.Control)
      do! sum.NextCustomer |> Async.AwaitEvent |> Async.Ignore }
    
  do 
    // When the applicataion starts, run the user interface
    app.Startup.Add(fun _ ->
      main () |> Async.StartImmediate )

// ------------------------------------------------------------------
  
module Main =
  [<STAThread>] 
  do 
    let win = new Window(Width = 815.0, Height = 625.0)
    (new App()).Run(win) |> ignore

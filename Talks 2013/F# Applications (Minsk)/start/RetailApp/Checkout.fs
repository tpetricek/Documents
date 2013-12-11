namespace Checkout

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

open Checkout.Domain

// ------------------------------------------------------------------
// Main application loop - repeatedly scan items & calculate summary

type App() as app =
  inherit Application()

  // Show the specified page
  let root = new Grid()
  let showPage ctrl = 
    root.Children.Clear()
    root.Children.Add(ctrl) |> ignore

  // TODO: Display final purchase after check.Completed occurs
  // (using Purchase.processPurchase and the Summary control)

  // TODO: Wait for sum.NextCustomer and then restart the loop
  // (This is a lot easier using asynchronous workflows)

  // TODO: Avoid blocking the GUI when adding item
  // (make the code in TescoData asynchronous)

  let main () = 
    app.MainWindow.Content <- root
    // Display the checkout line control
    let check = new CheckoutLine()
    showPage(check.Control) 
      
  do 
    // When the applicataion starts, run the user interface
    app.Startup.Add(fun _ ->
      main () )

// ------------------------------------------------------------------
  
module Main =
  [<STAThread>] 
  do 
    let win = new Window(Width = 815.0, Height = 625.0)
    (new App()).Run(win) |> ignore

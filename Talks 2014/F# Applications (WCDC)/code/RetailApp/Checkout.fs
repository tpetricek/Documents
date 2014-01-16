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

  let workflow () = async {
    app.MainWindow.Content <- root
    
    // Repeatedly process individual purchases
    while true do
      // Display the checkout line control
      let check = new CheckoutLine()
      showPage(check.Control) 
    
      // Wait until the checkout line completes
      let! items = Async.AwaitEvent check.Completed

      // Transform LineItem data to final aggregated purchase
      let final = Purchase.processPurchase items
    
      // Display the final bill control
      let sum = new Summary(final)
      showPage(sum.Control)  
      do! Async.AwaitEvent sum.NextCustomer }
    
  do 
    // When the applicataion starts, run the user interface
    app.Startup.Add(fun _ ->
      workflow () |> Async.StartImmediate )

// ------------------------------------------------------------------
  
module Main =
  [<STAThread>] 
  do 
    let win = new Window(Width = 815.0, Height = 625.0)
    (new App()).Run(win) |> ignore

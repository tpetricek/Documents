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

  let workflow () = async {
    app.MainWindow.Content <- root


    // Display the checkout line control
    let check = new CheckoutLine()
    showPage(check.Control) 
    
    // Wait until the checkout line completes
    let! items = Async.AwaitEvent check.Completed

    // Call processPurchase function from the Purchase module
    // to transform data in the LineItem-based format to the
    // final bill with aggregate quantities
    let final = SaleOperations.completeSale items
    
    // Display the final bill control
    let sum = new Summary(final)
    showPage(sum.Control)  
    let! _ = Async.AwaitEvent sum.NextCustomer
    
    // Close the application now...
    Application.Current.MainWindow.Close() } 

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

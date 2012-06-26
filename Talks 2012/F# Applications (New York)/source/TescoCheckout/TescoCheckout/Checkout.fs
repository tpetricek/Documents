namespace Checkout

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

open Checkout.Domain

type App() as app =
  inherit Application()

  let workflow () = async {
    let root = new Grid()
    app.MainWindow.Content <- root

    // Show the specified page
    let showPage ctrl = 
      root.Children.Clear()
      root.Children.Add(ctrl) |> ignore

    // TASK #1: Modify the asynchronous workflow to restart after
    // the 'NextCustomer' event. It should go back and create
    // new 'CheckoutLine' (to process the next customer)
    // This can be done either using loop or using recursion.

    // Display the checkout line control
    let check = new CheckoutLine()
    showPage(check.Control) 
    
    // Wait until the checkout line completes
    let! items = Async.AwaitEvent check.Completed

    // Call processPurchase function from the Purchase module
    // to transform data in the LineItem-based format to the
    // final bill with aggregate quantities
    let final = Purchase.processPurchase items
    
    // Display the final bill control
    let sum = new Summary(final)
    showPage(sum.Control)  
    let! _ = Async.AwaitEvent sum.NextCustomer
    
    // Display "fraud detected" message
    let msg = new Message("Fraud detected!")
    showPage(msg.Control)
    let! _ = Async.AwaitEvent msg.Completed
    
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

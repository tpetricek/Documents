namespace Checkout

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Media

type App() as app =
  inherit Application()

  let workflow = async {
    let! _ = app.Startup |> Async.AwaitEvent
    let root = new Grid()
    app.RootVisual <- root

    // Show the specified page
    let showPage ctrl = 
      root.Children.Clear()
      root.Children.Add(ctrl)

    while true do
      let check = new CheckoutLine()
      showPage(check) 
      let! items = check.Completed |> Async.AwaitEvent

      let! handle = 
        async { return Purchase.processPurchase items }
        |> Async.StartChild
      let! final = handle

      let sum = new Summary(final)
      showPage(sum)
      do! sum.NextCustomer |> Async.AwaitEvent }

  do workflow |> Async.StartImmediate
  













// ------------------------------------------------------------------
// Step 1 - Turning it into a simple workflow
(*
  let workflow = async {
    let! _ = app.Startup |> Async.AwaitEvent
    let root = new Grid()
    app.RootVisual <- root

    // Show the specified page
    let showPage ctrl = 
      root.Children.Clear()
      root.Children.Add(ctrl)

    showPage(new CheckoutLine()) }

  do workflow |> Async.StartImmediate
*)


// ------------------------------------------------------------------
// Step 2 - Add multiple steps to the workflow
(*
  let workflow = async {
    let! _ = app.Startup |> Async.AwaitEvent
    let root = new Grid()
    app.RootVisual <- root

    // Show the specified page
    let showPage ctrl = 
      root.Children.Clear()
      root.Children.Add(ctrl)

    // Read products from checkout line
    let ch = new CheckoutLine()
    showPage(ch)
    let! items = ch.Completed |> Async.AwaitEvent
    
    // Display summary bill
    let purchase = Purchase.processPurchase items
    let sum = new Summary(purchase)
    showPage(sum) }

  do workflow |> Async.StartImmediate
*)

// ------------------------------------------------------------------
// Final step - Workflow with while loop
(*
  let workflow = async {
    // Initialize application after startup
    let! _ = app.Startup |> Async.AwaitEvent
    let root = new Grid()
    app.RootVisual <- root

    // Show the specified page
    let showPage ctrl = 
      root.Children.Clear()
      root.Children.Add(ctrl)
    
    // Main control-flow loop
    while true do 
      // Read products from checkout line
      let line = new CheckoutLine()
      showPage(line)
      let! linePurchase = line.Completed |> Async.AwaitEvent
      let finalPurchase = Purchase.processPurchase linePurchase

      // Display summary bill
      let summary = new Summary(finalPurchase)
      showPage(summary)
      do! summary.NextCustomer |> Async.AwaitEvent }

  // Start the application workflow
  do workflow |> Async.StartImmediate
*)
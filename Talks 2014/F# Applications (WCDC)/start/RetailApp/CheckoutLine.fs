namespace Checkout

open System
open System.Windows 
open System.Windows.Controls
open System.Windows.Media
open System.Collections.ObjectModel

open Checkout.Domain

// ------------------------------------------------------------------

/// Class used for displaying items using WPF
type ItemView(itm:LineItem) = 
  member x.Item = itm

  /// Returns the URL to be used for the product preview
  member x.ImageSource =
    match itm with
    | SaleLineItem(idx, Product(_, _, pic, _), _) -> pic
    | _ -> ""
  
  /// Returns detailed formatted information about item
  member x.Text = 
    match itm with
    | CancelLineItem(idx) -> sprintf "Cancel item: %d" idx
    | SaleLineItem(idx, Product(_, name, _, price), q) -> 
        sprintf "Item %d: %s ($%M) x %d" idx name price (int q)


// ------------------------------------------------------------------
// WPF Control implementing checkout line

type CheckoutLine() =
  // Boilerplate code to load the user interface (WPF) and 
  // find all user interface controls that we need to use
  let uri = new System.Uri("/RetailApp;component/CheckoutLine.xaml", UriKind.Relative)
  let ctl = Application.LoadComponent(uri) :?> UserControl

  let (?) (this : Control) (prop : string) : 'T =
    this.FindName(prop) :?> 'T

  let finishBtn : Button  = ctl?FinishButton
  let addBtn    : Button  = ctl?AddButton
  let cancelBtn : Button  = ctl?CancelButton
  let itemsList : ListBox = ctl?ListItems
  let codeBox   : TextBox = ctl?ProductCode
  let quantBox  : TextBox = ctl?Quantity

  // State of the checkout line
  let items = new ObservableCollection<_>()
  do itemsList.DataContext <- items
  
  // ------------------------------------------------------------------
  // Event handlers for adding/cancelling items

  let addProduct() = 
    let res = Products.Lookup(codeBox.Text.Trim())
    match res with
    | Some(prod) -> 
        let newId = items.Count
        items.Add(ItemView(SaleLineItem(newId, prod, decimal quantBox.Text)))
        codeBox.Text <- ""
    | _ -> () 

  // ------------------------------------------------------------------
  // Register event handlers in the constructor code
  // (for adding and removing items)

  // Remove the selected item (if it is a SaleLineItem)
  do cancelBtn.Click.Add(fun _ ->
    if itemsList.SelectedItem <> null then
      match (itemsList.SelectedItem :?> ItemView).Item with
      | SaleLineItem(idx, _, _) -> 
          items.Add(ItemView(CancelLineItem(idx)))
      | _ -> ())

  // Add product when Add clicked on received \r from scanner
  do codeBox.KeyDown.Add(fun ke ->
    if ke.Key = Input.Key.Enter then
      addProduct() )

  do addBtn.Click.Add(fun _ -> 
      addProduct() )

  do ctl.Loaded.Add(fun _ -> codeBox.Focus() |> ignore)

  // Trigger completed event when Finish clicked
  let completed = new Event<_>()
  do finishBtn.Click.Add(fun _ -> 
    completed.Trigger [ for itm in items -> itm.Item] )

  member x.Completed = completed.Publish
  member x.Control = ctl
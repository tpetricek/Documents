namespace Checkout

open System
open System.Windows 
open System.Windows.Controls
open System.Windows.Media
open System.Collections.ObjectModel

// ------------------------------------------------------------------
// Class used for displaying items using WPF

type ItemView(itm:LineItem) = 
  member x.Item = itm
  override x.ToString() = 
    match itm with
    | CancelLineItem(idx) -> sprintf "Cancel item: %d" idx
    | SaleLineItem(idx, Product(_, name, price), q) -> 
        sprintf "Item %d: %s ($%M) x %d" idx name price (int q)

// ------------------------------------------------------------------
// WPF Control implementing checkout line

type CheckoutLine() as this =
  inherit UserControl()
  let uri = new System.Uri("/Checkout;component/CheckoutLine.xaml", UriKind.Relative)
  do Application.LoadComponent(this, uri)

  // Find user interface items
  let (?) (this : Control) (prop : string) : 'T =
    this.FindName(prop) :?> 'T

  let finishBtn : Button  = this?FinishButton
  let addBtn    : Button  = this?AddButton
  let cancelBtn : Button  = this?CancelButton
  let itemsList : ListBox = this?ListItems
  let codeBox   : TextBox = this?ProductCode
  let quantBox  : TextBox = this?Quantity

  // State of the checkout line
  let items = new ObservableCollection<_>()
  let mutable index = 1
  do itemsList.DataContext <- items
  
  // Event handlers for adding/cancelling items

  let addProduct() =
    match Products.Lookup(codeBox.Text.Trim()) with
    | Some(prod) -> 
        items.Add(ItemView(SaleLineItem(index, prod, decimal quantBox.Text)))
        codeBox.Text <- ""
        index <- index + 1
    | _ -> ()

  do cancelBtn.Click.Add(fun _ ->
    if itemsList.SelectedItem <> null then
      match (itemsList.SelectedItem :?> ItemView).Item with
      | SaleLineItem(idx, _, _) -> 
          items.Add(ItemView(CancelLineItem(idx)))
      | _ -> ())

  // Add product when Add clicked on received \r from scanner
  do codeBox.TextInput.Add(fun te ->
    if te.Text.EndsWith("\r") then addProduct())
  do addBtn.Click.Add(fun _ -> addProduct())
  
  // Trigger completed event when Finish clicked
  let completed = new Event<_>()
  do finishBtn.Click.Add(fun _ -> 
    completed.Trigger [ for itm in items -> itm.Item] )

  member x.Completed = completed.Publish
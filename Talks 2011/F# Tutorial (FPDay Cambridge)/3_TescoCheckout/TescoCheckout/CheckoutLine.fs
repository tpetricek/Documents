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
  member x.ImageSource =
    match itm with
    | SaleLineItem(idx, Product(_, _, pic, _), _) -> pic
    | _ -> ""
    
  member x.Text = x.ToString()
  override x.ToString() = 
    match itm with
    | CancelLineItem(idx) -> sprintf "Cancel item: %d" idx
    | SaleLineItem(idx, Product(_, name, _, price), q) -> 
        sprintf "Item %d: %s ($%M) x %d" idx name price (int q)

// ------------------------------------------------------------------
// WPF Control implementing checkout line

type CheckoutLine() =
  let uri = new System.Uri("/TescoCheckout;component/CheckoutLine.xaml", UriKind.Relative)
  let ctl = Application.LoadComponent(uri) :?> UserControl

  // Find user interface items
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
  let mutable index = 1
  do itemsList.DataContext <- items
  
  // Event handlers for adding/cancelling items

  let addProduct() =
    match Products.BackupLookup(codeBox.Text.Trim()) with
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
  member x.Control = ctl
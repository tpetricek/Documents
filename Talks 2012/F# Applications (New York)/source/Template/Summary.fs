namespace Checkout

open System
open System.Windows 
open System.Windows.Controls
open System.Windows.Media

open Checkout.Domain

// ------------------------------------------------------------------
// Class used for displaying items using WPF

type ProductSummary(prod, count:int) = 
  let (Product(code, name, price, _)) = prod
  member x.Name = name
  member x.Price = price
  member x.Quantity = count
  member x.Total = price * (decimal count)

// ------------------------------------------------------------------
// WPF Control for showing final bill

type Summary(purchase:FinalSale) =
  let uri = new System.Uri("/TescoCheckout;component/Summary.xaml", UriKind.Relative)
  let ctl = Application.LoadComponent(uri) :?> UserControl

  let (?) (this : Control) (prop : string) : 'T =
    this.FindName(prop) :?> 'T

  let nextBtn : Button = ctl?NextButton
  let itemsList : ListBox = ctl?ListItems
  do itemsList.DataContext <- 
       [ for (KeyValue(p, q)) in purchase -> ProductSummary(p, q) ]

  member x.NextCustomer = nextBtn.Click |> Event.map ignore
  member x.Control = ctl
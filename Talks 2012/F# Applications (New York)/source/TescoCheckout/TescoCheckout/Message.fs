namespace Checkout

open System
open System.Windows 
open System.Windows.Controls
open System.Windows.Media

open Checkout.Domain

// ------------------------------------------------------------------
// WPF Control for showing messages

type Message(message:string) =
  let uri = new System.Uri("/TescoCheckout;component/Message.xaml", UriKind.Relative)
  let ctl = Application.LoadComponent(uri) :?> UserControl

  let (?) (this : Control) (prop : string) : 'T =
    this.FindName(prop) :?> 'T

  let nextBtn : Button = ctl?NextButton
  let label : Label = ctl?InfoLabel
  do label.Content <- message

  member x.Completed = nextBtn.Click |> Event.map ignore
  member x.Control = ctl
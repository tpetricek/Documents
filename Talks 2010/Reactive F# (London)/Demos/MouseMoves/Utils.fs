module Utils
open System.Windows.Controls

let (?) (this : Control) (prop : string) : 'T =
  this.FindName(prop) :?> 'T

namespace FunJS

// ----------------------------------------------------------------------------
// Useful extensions
// ----------------------------------------------------------------------------

[<AutoOpen>]
module FunJSExtensions = 

  [<JS; JSEmit("return Math.floor({0});")>]
  let floor (n:int) = n


[<ReflectedDefinition>]
module Program

open FunScript
open FSharp.Data

let main() = 
  ()
































// ------------------------------------------------------------------
let components = 
  FunScript.Data.Components.DataProviders @ 
  FunScript.Interop.Components.all
do Runtime.Run(components=components, directory="Web")
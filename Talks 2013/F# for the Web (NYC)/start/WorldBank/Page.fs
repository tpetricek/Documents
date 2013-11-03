[<ReflectedDefinition>]
module Program

open FunScript
open FSharp.Data

// Reference JavaScript libraries
type lib = TypeScript.Api<"../Typings/lib.d.ts">

// Main function
let main() = 
  ()

























// ------------------------------------------------------------------
let components = 
  FunScript.Data.Components.DataProviders @ 
  FunScript.Interop.Components.all
do Runtime.Run(components=components, directory="Web")
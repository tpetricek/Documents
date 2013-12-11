[<ReflectedDefinition>]
module Program

open FunScript
open FSharp.Data

type lib = TypeScript.Api<"C:/Typings/lib.d.ts">

// DEMO: WorldBank type Provider & typing for JQuery
// DEMO: Create a list of countries (countries : unit -> Country[])

let main() = 
  // DEMO: Create checkboxes for all the countries
  // DEMO: Add rendering function
  // DEMO: Add event handlers to check boxes
  ()

























// ------------------------------------------------------------------
let components = 
  FunScript.Data.Components.DataProviders @ 
  FunScript.Interop.Components.all
do Runtime.Run(components=components, directory="Web")
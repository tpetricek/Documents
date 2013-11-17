[<ReflectedDefinition>]
module Program

open FunScript
open FSharp.Data

// Initializataion & Helpers
type MovieDb = ApiaryProvider<"themoviedb">

type j = TypeScript.Api<"../Typings/jquery.d.ts">
type lib = TypeScript.Api<"../Typings/lib.d.ts">
let jQuery (command:string) = j.jQuery.Invoke(command)
let (?) jq name = jQuery("#" + name)

type System.Object with
  member x.asJQuery() : j.JQuery = unbox x


// Main client-side function
let main() = 
  ()













// ------------------------------------------------------------------
let components = 
  FunScript.Interop.Components.all @
  FunScript.Data.Components.DataProviders  
do Runtime.Run(components=components, directory="Web")
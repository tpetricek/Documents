[<ReflectedDefinition>]
module Program

open FunScript
open FSharp.Data

// ------------------------------------------------------------------

type j = TypeScript.Api<"../Typings/jquery.d.ts">
let jQuery (command:string) = j.jQuery.Invoke(command)
let (?) jq name = jQuery("#" + name)
type System.Object with
  member x.asJQuery() : j.JQuery = unbox x

// ------------------------------------------------------------------

let root = "http://cf2.imgobject.com/t/p/w92/"

let main() = 
  // Movie search
  let search (term:string) = 
    ()    

  // Setup search handler
  jQuery?searchButton.click(fun () ->
    let id = jQuery?searchInput.``val``()
    search (unbox id) )


// ------------------------------------------------------------------
// Compile to JavaScript and run simple web server

let components = 
  FunScript.Interop.Components.all @
  FunScript.Data.Components.DataProviders  
do Runtime.Run(components=components, directory="Web")
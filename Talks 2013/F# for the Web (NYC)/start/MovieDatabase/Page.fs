[<ReflectedDefinition>]
module Program

open FunScript
open FSharp.Data

// Initializataion
type MovieDb = ApiaryProvider<"themoviedb">

type j = TypeScript.Api<"../Typings/jquery.d.ts">
let jQuery (command:string) = j.jQuery.Invoke(command)
let (?) jq name = jQuery("#" + name)

type System.Object with
  member x.asJQuery() : j.JQuery = unbox x


// Main client-side function
let main() = 
  // Movie search
  let search term = 
    jQuery?results.html("") |> ignore

    let body = jQuery("<div>").html("No results for: " + term).asJQuery()
    body.appendTo(jQuery?results) |> ignore

  // Setup event handler
  jQuery?searchButton.click(fun () ->
    let id = jQuery?searchInput.``val``() :?> string
    search id ) |> ignore














// ------------------------------------------------------------------
let components = 
  FunScript.Interop.Components.all @
  FunScript.Data.Components.DataProviders  
do Runtime.Run(components=components, directory="Web")
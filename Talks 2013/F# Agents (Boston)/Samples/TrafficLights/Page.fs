[<ReflectedDefinition>]
module Program

// ----------------------------------------------------------------------------
// Open FunScript namespaces and implement some simple helpers

open FunScript
open FunScript.TypeScript

module FunScriptUtils =

  // Use TypeScript type provider to import basic JavaScript libraries
  type j = FunScript.TypeScript.Api<"lib/jquery.d.ts">
  type lib = FunScript.TypeScript.Api<"lib/lib.d.ts">

  // Mini implementation of the Await operation for JQuery events
  type Async =
    static member AwaitJQueryEvent(f : ('T -> obj) -> j.JQuery) : Async<'T> = 
      Async.FromContinuations(fun (cont, econt, ccont) ->
        let named = ref None
        named := Some (f (fun v -> 
          (!named).Value.off() |> ignore
          cont v
          obj() )))
    static member AwaitJQueryEvent(f1 : ('T1 -> obj) -> j.JQuery, f2 : ('T2 -> obj) -> j.JQuery) : Async<Choice<'T1, 'T2>> = 
      Async.FromContinuations(fun (cont, econt, ccont) ->
        let named1 : j.JQuery option ref = ref None
        let named2 : j.JQuery option ref = ref None
        named1 := Some (f1 (fun v -> 
          (!named1).Value.off() |> ignore
          (!named2).Value.off() |> ignore
          cont (Choice1Of2 v)
          obj() ))
        named2 := Some (f2 (fun v -> 
          (!named1).Value.off() |> ignore
          (!named2).Value.off() |> ignore
          cont (Choice2Of2 v)
          obj() )) )
      
  // Dynamic operator for accessing named elements using JQuery
  let (?) (jq:j.JQueryStatic) name = jq.Invoke("#" + name)

// ----------------------------------------------------------------------------
// DEMO: Traffic lights using asynchronous workflows
// ----------------------------------------------------------------------------

open FunScriptUtils

/// Represents possible displayed lights
type Light = Red | Orange | Green | Nothing

/// Display the specified light
let showLight light = 
  j.jQuery?red.hide() |> ignore
  j.jQuery?orange.hide() |> ignore
  j.jQuery?green.hide() |> ignore
  match light with
  | Red -> j.jQuery?red.show() |> ignore
  | Orange -> j.jQuery?orange.show() |> ignore
  | Green -> j.jQuery?green.show() |> ignore
  | Nothing -> ()

/// Display Green, Orange and Red sequence
let rec trafficLight progress = async { 
  let states = [| Green; Orange; Red |]
  while true do
    for s in states do    
      let! e = progress
      showLight s }

/// Start the application & register handlers
let main() = 
  let doc = j.jQuery?document
  doc.ready(fun _ ->    

    // Play traffic lights automatically
    j.jQuery?auto.click(fun _ -> 
      trafficLight (Async.Sleep(1000))
      |> Async.StartImmediate ) |> ignore

    // Play traffic lights automatically
    j.jQuery?manual.click(fun _ -> 
      trafficLight (Async.AwaitJQueryEvent(j.jQuery?next.click))
      |> Async.StartImmediate )
      
       )

// ----------------------------------------------------------------------------
// Translate to JavaScript & run in a web browser

do Runtime.Run(components=Interop.Components.all)
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
    static member AwaitJQueryEvents(f1 : ('T1 -> obj) -> j.JQuery, f2 : ('T2 -> obj) -> j.JQuery) : Async<Choice<'T1, 'T2>> = 
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
// DEMO: Simple drawing application using asynchronous workflows
// ----------------------------------------------------------------------------

open FunScriptUtils

/// Moves the specified <div> element to a given location
let moveRectangle (el:j.JQuery) (x1, y1) (x2, y2) = 
  let abs n = if n < 0.0 then -n else n
  el.css("left", min x1 x2) |> ignore
  el.css("top", min y1 y2) |> ignore
  el.css("width", abs (x1 - x2)) |> ignore
  el.css("height", abs (y1 - y2)) |> ignore


/// Represents the drawing state - we're waiting for mousemove
/// (to update the state) or mouseup (to finish drawing)
let rec drawing (el:j.JQuery) (dx, dy) = async {
  let! e = Async.AwaitJQueryEvents(j.jQuery?canvas.mousemove, j.jQuery?canvas.mouseup)
  match e with
  | Choice1Of2 e ->
      moveRectangle el (dx, dy) (e.pageX, e.pageY)
      return! drawing el (dx, dy)
  | Choice2Of2 e ->
      el.addClass("rect") |> ignore
      el.removeClass("shadow") |> ignore
      moveRectangle el (dx, dy) (e.pageX, e.pageY)
      return! waiting() }

/// Represents the waiting state - waiting for a mousedown event
and waiting () = async {
  let! e = Async.AwaitJQueryEvent(j.jQuery?canvas.mousedown)
  let el = j.jQuery.Invoke("<div>")
  el.appendTo(j.jQuery?canvas) |> ignore
  el.addClass("shadow") |> ignore
  return! drawing el (e.pageX, e.pageY) }


/// Main client-side function 
let main() = 
  let doc = j.jQuery?document
  doc.ready(fun _ ->    
    waiting ()
    |> Async.StartImmediate )

// ----------------------------------------------------------------------------
// Translate to JavaScript & run in a web browser

do Runtime.Run(components=Interop.Components.all)
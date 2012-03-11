module SocialDrawing.App

open Pit
open Pit.Dom
open Pit.Async
open Pit.Javascript
open SocialDrawing.Dom

// -----------------------------------------------------------------------------

type Rectangle = 
  { X1 : int; Y1 : int
    X2 : int; Y2 : int
    Color : string }

// -----------------------------------------------------------------------------

let [<Js>] sendRectangle (value:Rectangle) = 
  let req = XMLHttpRequest()
  req.Open("POST", "/add", true)
  req.Send(JSON.stringify(value))

let [<Js>] getRectangles () : Async<Rectangle[]> = async {
  let req = XMLHttpRequest()
  req.Open("POST", "/get", true)
  let! resp = req.AsyncSend()
  return JSON.parse(resp) }

let [<Js>] createRectangle (rect:Rectangle) =    
  let el = createSelection()
  el.ClassName <- "rect"
  el.Style.BackgroundColor <- rect.Color
  moveElement el (rect.X1, rect.Y1) (rect.X2, rect.Y2)

// -----------------------------------------------------------------------------
  
[<DomEntryPoint; Js>]
let main() =        
  let div = document.GetElementById("canvas")
  div 
  |> Event.mousedown
  |> Event.add (fun args ->
      let rect = 
        { X1 = args.ClientX; Y1 = args.ClientY;
          X2 = args.ClientX + 10; Y2 = args.ClientY + 10; 
          Color = "red" }
      sendRectangle rect
      createRectangle rect )

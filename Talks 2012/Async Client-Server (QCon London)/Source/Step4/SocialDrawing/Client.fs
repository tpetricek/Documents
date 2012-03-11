module SocialDrawing.App

open Pit
open Pit.Dom
open Pit.Async
open Pit.Javascript
open SocialDrawing.Dom
open SocialDrawing.Colors

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

let [<Js>] updateLoop () = async {
  while true do 
    do! Async.Sleep(500)
    let! rects = getRectangles()
    cleanRectangles()
    rects |> Array.iter createRectangle }

let rec [<Js>] drawing selection start = async {
  let div = document.GetElementById("canvas")
  let! e = Async.AwaitEvent(Event.mouseup div, Event.mousemove div)
  match e with
  | Choice2Of2 e ->
      moveElement selection start (e.ClientX, e.ClientY)
      return! drawing selection start 
  | Choice1Of2 e ->
      removeRectangle selection
      let color = selectedColor()
      let rect = 
        { X1 = fst start; Y1 = snd start;
          X2 = e.ClientX; Y2 = e.ClientY; 
          Color = color }
      sendRectangle rect
      createRectangle rect
      return! waiting () }

and [<Js>] waiting () = async {
  let div = document.GetElementById("canvas")
  let! e = Async.AwaitEvent(Event.mousedown div)
  let selection = createSelection()
  return! drawing selection (e.ClientX, e.ClientY) }

// -----------------------------------------------------------------------------
  
[<DomEntryPoint; Js>]
let main() =        
  createPalette()
  updateLoop () |> Async.StartImmediate
  waiting () |> Async.StartImmediate

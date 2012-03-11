module SocialDrawing.Dom

open Pit
open Pit.Dom

let [<Js>] createSelection () = 
  let root = document.GetElementById("canvas")
  let el = document.CreateElement("div")
  el.Style.Position <- "absolute"
  root.AppendChild(el)
  el.ClassName <- "selection"
  el

let [<Js>] moveElement (el:DomElement) (x1, y1) (x2, y2) =
  let x, y = min x1 x2, min y1 y2
  let w, h = abs (x1 - x2), abs (y1 - y2)
  el.Style.Left <- x.ToString() + "px"
  el.Style.Top <- y.ToString() + "px"
  el.Style.Width <- w.ToString() + "px"
  el.Style.Height <- h.ToString() + "px"

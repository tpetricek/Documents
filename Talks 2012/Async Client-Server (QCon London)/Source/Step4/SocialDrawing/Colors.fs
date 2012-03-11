module SocialDrawing.Colors

open Pit
open Pit.Dom

let [<Js>] colors () = 
  [ let baseColors = 
      [ 136,0,21; 237,28,36; 255,127,40; 255,242,0; 
        34,177,76; 0,162,232; 63,72,204; 163,73,163 ]
    for shade in [ 32; 85; 170; 255 ] do
      yield (shade, shade, shade)
    yield! baseColors ]
  
let [<Js>] hex (r, g, b) = 
  let part r =
    let letters = [| "0"; "1"; "2"; "3"; "4"; "5"; "6"; "7"; "8"; "9"; "a"; "b"; "c"; "d"; "e"; "f" |]
    letters.[int (r/16)] + letters.[r%16]
  "#" + (part r) + (part g) + (part b)

let [<Js>] createPalette () = 
  let root = document.GetElementById("palette")
  let sel = document.CreateElement("div")
  sel.ClassName <- "selected"
  sel.Id <- "selected"
  sel.Style.BackgroundColor <- "#202020"
  root.AppendChild(sel)

  colors () |> List.rev |> List.iter (fun color ->
    let el = document.CreateElement("div")
    el.ClassName <- "color"
    let colorName = hex color
    el.Style.BackgroundColor <- colorName 
    el |> Event.click |> Event.add (fun _ ->
      sel.Style.BackgroundColor <- colorName)
    root.AppendChild(el) )

let [<Js>] selectedColor () =
  let sel = document.GetElementById("selected")
  sel.Style.BackgroundColor
  

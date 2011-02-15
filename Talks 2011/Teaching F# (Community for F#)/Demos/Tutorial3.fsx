// ----------------------------------------------------------------------------
// Recursion - generating drawings from lists
// ----------------------------------------------------------------------------

#load "Drawing.fs"
open System.Drawing
open FunctionalDrawing


// Writing recursive function to create chart

let chartBar title index height color = 
  let shiftY = height / 2.0f + 20.0f
  let bar = Fun.rectangle 50.0f height
  Fun.move (index * 100.0f) 0.0f 
    ( Fun.fillColor color (Fun.move 0.0f shiftY bar) $
      Fun.text title )

let rec plotData index data = 
  match data with 
  | (title, value, color)::rest ->
    plotData (index + 1) rest $
    chartBar title (float32 index) (float32 value * 10.0f) color
  | [] -> Fun.empty

let data =
  [ ("Sarkozy", 31, Color.OrangeRed);
    ("Royal", 26, Color.DarkOrange);
    ("Bayrou", 19, Color.Gold);
    ("Le Pen", 10, Color.OliveDrab) ]

plotData 0 data



// Processing data using higher-order functions

data 
  |> List.mapi (fun i (name, value, color) ->
    chartBar name (float32 i) (float32 value * 10.0f) color)
  |> List.reduce ($)



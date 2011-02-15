// ----------------------------------------------------------------------------
// Creating drawings using expressions
// ----------------------------------------------------------------------------

#load "Drawing.fs"
open System.Drawing
open FunctionalDrawing


// We can create simple expressions - even without the 'let' keyword!

Fun.circle 200.0f 

Fun.circle 200.0f $ 
  Fun.move 0.0f 150.0f (Fun.circle 100.0f)

Fun.fillColor Color.Gold (Fun.circle 200.0f) $
  Fun.fillColor Color.Red (Fun.circle 150.0f) $
  Fun.fillColor Color.Gold (Fun.move 0.0f 15.0f (Fun.circle 160.0f)) $
  Fun.fillColor Color.Green
    (Fun.move -50.0f  25.0f (Fun.circle 50.0f) $
     Fun.move  50.0f  25.0f (Fun.circle 50.0f)) 
      
Fun.move 0.0f 280.0f (Fun.fillColor Color.PowderBlue (Fun.circle 120.0f)) $
  Fun.move 0.0f 160.0f (Fun.fillColor Color.SkyBlue (Fun.circle 160.0f)) $
    Fun.fillColor Color.SteelBlue (Fun.circle 200.0f) 




// ----------------------------------------------------------------------------
// Using 'let' declarations to reuse parts of expressions

let portal = 
  Fun.rectangle 100.0f 100.0f $ 
  Fun.move 0.0f 50.0f (Fun.circle 100.0f)

let smallPortal = 
  Fun.fillColor Color.LightSlateGray
    (Fun.scale 0.5f 0.5f portal)

portal $
  ( Fun.move -120.0f 50.0f smallPortal $
    Fun.move 120.0f 50.0f smallPortal )




// ----------------------------------------------------------------------------
// Introducing functions using drawings

let chartBar title index height color = 
  let shiftY = height / 2.0f + 20.0f
  let bar = Fun.rectangle 50.0f height
  Fun.move (index * 100.0f) 0.0f 
    ( Fun.fillColor color (Fun.move 0.0f shiftY bar) $
      Fun.text title )

chartBar "Conservative" 0.0f 306.0f Color.SlateBlue $
chartBar "Labour" 1.0f 258.0f Color.DarkRed $
chartBar "Lib. Dem." 2.0f 57.0f Color.Goldenrod

// ----------------------------------------------------------------------------
// Introducing general recursion using 3D objects
// ----------------------------------------------------------------------------

#r "OpenTK.dll"
#r "OpenTK.GLControl.dll"
#load "functional3d.fs"

open Functional3D
open System.Drawing


// ----------------------------------------------------------------------------
// Introducing 3D library

Fun.cube

Fun.color Color.DarkRed Fun.cone $
( Fun.color Color.Goldenrod 
    (Fun.translate (0.0, 0.0, 1.0) Fun.cylinder) )

// ----------------------------------------------------------------------------
// 3D recursive tree (written by James Margetson)

// Creating trunks and fruits

let trunk width length = 
  Fun.cylinder
    |> Fun.translate (0.0,0.0,0.5) 
    |> Fun.scale (width,width,length)  
        
let fruit size = 
  Fun.sphere
    |> Fun.color Color.Red 
    |> Fun.scale (size, size, size)

let example = 
  trunk 1.0 5.0 $ fruit 2.0


// Generating recursive tree
Fun.quality <- 10

let rec tree trunkLength trunkWidth w n = 
  if n = 0 then
    // At the end of the recursion - we just return fruit
    fruit (3.0 * trunkWidth)
  else 
    // Local function to generate rotated tree (recursively)
    let branch angleX angleY = 
      let branchLength = trunkLength * 0.92 
      let branchWidth = trunkWidth * 0.65 

      // Recursively generate smaller tree
      tree branchLength branchWidth w (n-1) 
        |> Fun.rotate (angleX, angleY, 0.0) 
        |> Fun.translate (0.0,0.0,trunkLength)

    // Generate trunk and four smaller trees
    trunk trunkWidth trunkLength $
      branch w 0.0 $ branch -w 0.0 $
      branch 0.0 w $ branch 0.0 -w


let depth = 4

let plant = 
  tree 4.0 0.8 40.0 depth
  |> Fun.rotate (90.0, 180.0, 90.0)
  |> Fun.translate (0.0, -6.0, 0.0)


Fun.resetRotation()
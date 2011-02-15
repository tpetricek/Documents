// ----------------------------------------------------------------------------
// Introducing general recursion using 3D objects
// ----------------------------------------------------------------------------

#r "references\\OpenTK.dll"
#r "references\\OpenTK.GLControl.dll"
#load "functional3d.fs"
open Functional3D
open System.Drawing


// ----------------------------------------------------------------------------
// Introducing 3D library

Fun.cube


Fun.color Color.DarkRed Fun.cone $
( Fun.color Color.Goldenrod 
    (Fun.translate (0.0, 0.0, 1.0) Fun.cylinder) )


let tower x z = 
  (Fun.cylinder
     |> Fun.scale (1.0, 1.0, 3.0) 
     |> Fun.translate (0.0, 0.0, 1.0)
     |> Fun.color Color.DarkGoldenrod ) $ 
  (Fun.cone 
     |> Fun.scale (1.3, 1.3, 1.3) 
     |> Fun.translate (0.0, 0.0, -1.0)
     |> Fun.color Color.Red )
  |> Fun.rotate (90.0, 0.0, 0.0)
  |> Fun.translate (x, 0.5, z)

tower -2.0 0.0 $ tower 2.0 0.0



// ----------------------------------------------------------------------------
// We can create interesting 3D objects

let sizedCube height = 
  Fun.cube 
    |> Fun.scale (0.5, height, 1.0) 
    |> Fun.translate (-0.5, height/2.0 - 1.0, 0.0)

let twoCubes =
  sizedCube 0.8 $ (sizedCube 1.0 |> Fun.translate (0.5, 0.0, 0.0))

let block = 
  [ for offset in -4.0 .. +4.0 ->
      twoCubes |> Fun.translate (offset, 0.0, 0.0) ]
  |> Seq.reduce ($)
  |> Fun.scale (0.5, 2.0, 0.3)
  |> Fun.color Color.DarkGray
  
let wall offs rotate = 
  let rotationArg    = if rotate then (0.0, 90.0, 0.0) else (0.0, 0.0, 0.0)
  let translationArg = if rotate then (offs, 0.0, 0.0) else (0.0, 0.0, offs)
  block |> Fun.rotate rotationArg |> Fun.translate translationArg

let shape = 
    tower -2.0 -2.0  $ tower 2.0 -2.0 $ 
    tower -2.0  2.0  $ tower 2.0 2.0  $
    wall  -2.0 true  $ wall  2.0 true $
    wall  -2.0 false $ wall  2.0 false



// ----------------------------------------------------------------------------
// 3D recursive tree (written by James Margetson)

Fun.quality <- 10


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


let depth = 1

let plant = 
  tree 4.0 0.8 40.0 depth
  |> Fun.rotate (90.0, 180.0, 90.0)
  |> Fun.translate (0.0, -6.0, 0.0)


Fun.resetRotation()


#r "references/OpenTK.dll"
#r "references/OpenTK.GLControl.dll"
#load "functional3d.fs"

open Functional3D
open System.Drawing

// ------------------------------------------------------------------
// Basic shapes and operations

// The 'Fun' module provides several primitive 3D objects
// (when using F# interactive, just select and evaluate them line-by-line)
// (use Q/W, A/S and Z/X keys to rotate the view)

Fun.cone
Fun.sphere
Fun.cube

// We can use combinators to change colors and other properties

Fun.cube 
 |> Fun.color Color.OliveDrab 

// We can use '$' to compose multiple objects into single one

Fun.translate (-1.0, 0.0, 0.0) Fun.cube $
Fun.translate (1.0, 0.0, 0.0) Fun.cube

// Create a small tower and rotate it

Fun.color Color.DarkRed Fun.cone $
( Fun.color Color.Goldenrod 
    (Fun.translate (0.0, 0.0, 1.0) Fun.cylinder) )

// ------------------------------------------------------------------
// Writing functions

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

// Create one tower
tower 0.0 0.0

// Now we can easily compose towers!
tower -2.0 0.0 $ tower 2.0 0.0
                                                                                                                                
// ------------------------------------------------------------------
// Generating and processing lists

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
  
  
// ------------------------------------------------------------------
// Putting things together

let wall offs rotate = 
  let rotationArg = if rotate then (0.0, 90.0, 0.0) else (0.0, 0.0, 0.0)
  let translationArg = if rotate then (offs, 0.0, 0.0) else (0.0, 0.0, offs)
  block |> Fun.rotate rotationArg |> Fun.translate translationArg

tower -2.0 -2.0 $ tower 2.0 -2.0 $ 
  tower -2.0 2.0 $ tower 2.0 2.0 $
  wall -2.0 true $ wall 2.0 true $
  wall -2.0 false $ wall 2.0 false
  

// ------------------------------------------------------------------
// Recursion 

let pattern = 
  [| [| [| 1; 1; 1; |]; [| 1; 0; 1 |]; [| 1; 1; 1 |] |]
     [| [| 1; 0; 1; |]; [| 0; 0; 0 |]; [| 1; 0; 1 |] |]
     [| [| 1; 1; 1; |]; [| 1; 0; 1 |]; [| 1; 1; 1 |] |] |]
  |> Array3D.fromCube

let rec generate depth = 
  [ for x in -1 .. 1 do
    for y in -1 .. 1 do
    for z in -1 .. 1 do
      if pattern.[x, y, z] = 1 then 
        let size = 3.0 ** float depth
        let ofs = float x * size, float y * size, float z * size
        let sub = if depth = 0 then Fun.cube
                  else generate (depth - 1) 
        yield Fun.translate ofs sub ]
  |> List.reduce ($)
  |> Fun.color Color.ForestGreen

// Generate fractal with various level of detail
  
Fun.setDistance(-20.0)

generate 0
generate 1

Fun.setDistance(-60.0)
generate 2

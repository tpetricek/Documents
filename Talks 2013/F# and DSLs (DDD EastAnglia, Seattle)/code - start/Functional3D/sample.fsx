#r "references/OpenTK.dll"
#r "references/OpenTK.GLControl.dll"
#load "functional3d.fs"

open Functional3D
open System.Drawing

// ------------------------------------------------------------------

// SAMPLE
//   Create a 3D primitive

Fun.cube




// DEMO 
//   Create red cone, translate by 0,0,-1
//   Compose with yellow cylinder

// DEMO
//   Building towers - create tower, compose towers

// DEMO
//   Composing cubes, generating for offsets -4 .. +4
                                                                                                                  
// DEMO
//   Building walls, composing 3D objects
//   (towers from -2,-2 .. +2, +2 with walls -2,true .. +2, false)
  



















// ------------------------------------------------------------------
// Recursion 
// ------------------------------------------------------------------

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
generate 2

Fun.setDistance(-60.0)
generate 2

// ------------------------------------------------------------------
// Trees are an example of recursive structure
// ------------------------------------------------------------------

let random = System.Random()

let noise k x =
  x + (k * x * (random.NextDouble() - 0.5))

let color() = 
  [| Color.Red; Color.Orange; 
     Color.Yellow |].[random.Next 3]

let trunk (width,length) = 
  Fun.cylinder
  |> Fun.translate (0.0,0.0,0.5) |> Fun.scale (width,width,length)  
        
let fruit (size) = 
  Fun.sphere
  |> Fun.color (color()) |> Fun.scale (size,size,size)

let example = trunk (1.0,5.0) $ fruit 2.0


// Recursive tree
let rec tree trunkLength trunkWidth w n = 
  let moveToEndOfTrunk = Fun.translate (0.0,0.0,trunkLength)
  if n <= 1 then
    trunk (trunkWidth,trunkLength) $  // branch and end with
    (fruit (3.0 * trunkWidth) |> moveToEndOfTrunk)  // fruit
  else 
    // generate branch
    let branch angleX angleY = 
      let branchLength = trunkLength * 0.92 |> noise 0.2  // reduce length
      let branchWidth  = trunkWidth  * 0.65 |> noise 0.2  // reduce width
      tree branchLength branchWidth w (n-1) 
      |> Fun.rotate (angleX,angleY,0.0) |> moveToEndOfTrunk
      
    trunk (trunkWidth,trunkLength)  // branch and follow by several
      $ branch  w  0.0              // smaller branches with rotation +/- w
      $ branch -w  0.0
      $ branch 0.0   w
      $ branch 0.0  -w

let plant = 
  tree 4.0(*long*) 0.8(*wide*) 40.0(*angle*) 4(*levels*)
  |> Fun.rotate (90.0, 180.0, 90.0)
  |> Fun.translate (0.0, -6.0, 0.0)


Fun.resetRotation()

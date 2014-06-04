#r "references/OpenTK.dll"
#r "references/OpenTK.GLControl.dll"
#load "functional3d.fs"

open Functional3D
open System.Drawing

// ------------------------------------------------------------------

( Fun.color Color.Yellow Fun.cylinder ) $
( Fun.cone
  |> Fun.color Color.Red 
  |> Fun.translate (0.0, 0.0, -1.0) )

// ------------------------------------------------------------------

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

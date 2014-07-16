#r "references/OpenTK.dll"
#r "references/OpenTK.GLControl.dll"
#load "functional3d.fs"

open Functional3D
open System.Drawing

// ------------------------------------------------------------------
// The first thing you can do is to build a (Christmas) tree!
// Have a look at the sample image in "images/tree.png".
// ------------------------------------------------------------------

// The tree is a bunch of cones that are connected at the top.
// Start by creating a single cone using 'Fun.cone'

Fun.cylinder
Fun.cube

// When a window appears, you can use the keys Q/W, A/S and Z/X
// to rotate the view using various axes and you can also use
// + and - to zoom in and out.

// Now, we need a function that creates a cone and scales it. 
// Given 2.0, we want a cone that's 2x bigger in X and Y, but
// stays the same in the Z direction. To do that, use
// 'Fun.scale'. We also need to move the cone, so that they
// are not all in the center. Move it in the Z direction by
// the specified offset.

let treePart scale offset = 
  failwith "TODO"

// Now we need to generate tree parts and put them together.
// You can use a sequence expression to generate a list of 
// cones:

[ for i in 0.0 .. 10.0 -> Fun.translate (0.0, i, 0.0) Fun.cube ]

// When you run this, F# interactive will only display the last
// object that was created. So, we need to compose all of the 
// objects together using the '$' operator. The easiest way is 
// to use the 'Seq.reduce' function which combines all values using
// the given operator:

[ 1; 2; 3; 4 ] |> Seq.reduce (+)

// This will be a bit slow, because there are too many triangles :-)

[ for i in 0.0 .. 0.1 .. 6.3 -> 
    Fun.translate (0.0, sin i, cos i) Fun.sphere ]
|> Seq.reduce ($)

// Now you hopefuly have something that looks like a Chirstmas tree!
// The last bit is to add a trunk. You can create one using 
// 'Fun.cylinder' (and then using 'Fun.color' and 'Fun.translate' 
// to move it to the right place)

// As a bonus, you can also decorate the tree with colorful 
// spheres using Fun.sphere :-)


// ------------------------------------------------------------------
// You can easily add some animation to your 3D objects too!
// To create animated objects, you can write code as a function
// that takes the current time (float) and returns the shape
// for a given time. 
// ------------------------------------------------------------------

// Here, we create a simple sphere that is rotating around the
// center and is pulsing as well:

let rotatingSphere time = 
  let x = sin (time / 10.0)
  let y = cos (time / 10.0)
  let s = 1.0 + (sin (time / 100.0))
  Fun.sphere
  |> Fun.scale (s, s, s)
  |> Fun.translate (x, y, 0.0) 

// The 'Fun.animat' function returns a cancellation token so
// that you can stop the process (but if you just call
// Fun.animate with a new version of your shape, it will 
// automatically stop the old one).

let ct = Fun.animate rotatingSphere
ct.Cancel()


// ------------------------------------------------------------------
// More challenging task is to create a 3D fractal. Have a look
// at the sample in "images/cube.png". To build this one, we need
// to create cubes according to a pattern and then use
// recursion to apply the same pattern to sub-cubes in the big one.
// ------------------------------------------------------------------

// The 3D pattern that we want to follow is below. We are using
// a simple 3D array 3x3x3 where '1' means that the part of the
// pattern should be filled and '0' means that the part of the 
// pattern should be empty:

let pattern = 
  [| [| [| 1; 1; 1; |]; [| 1; 0; 1 |]; [| 1; 1; 1 |] |]
     [| [| 1; 0; 1; |]; [| 0; 0; 0 |]; [| 1; 0; 1 |] |]
     [| [| 1; 1; 1; |]; [| 1; 0; 1 |]; [| 1; 1; 1 |] |] |]
  |> Array3D.fromCube

// The 'fromCube' helper creates an array where the indices are
// from -1 to 1 (so the 0,0,0 offset is the center):

pattern.[-1,-1,-1]
pattern.[0,0,0]
pattern.[1,1,1]

// Now, the first step is to write a (non-recursive) function 'generate'
// that iterates over x, y, z ranging from -1 to 1 and generates a cube
// if the 'pattern' array contains '1' at the specified location. Then
// you also need to move the cube to the right offset using 
// 'Fun.translate':

let generate () = 
  [ for x in -1 .. 1 do
      for y in -1 .. 1 do
        for z in -1 .. 1 do
          // TODO: Check for the pattern & translate
          yield Fun.cube ]
  |> Seq.reduce ($)            

// The next step is to make the function recursive! Rather than 
// building 'Fun.cube' at each step, you can call the 'generate'
// function recursively to create a smaller fractal (don't forget
// to stop after a few iterations :-)). Then you also need to 
// re-scale the smaller cube, so that it is 1/3 of the size of 
// the new one in all dimensions!

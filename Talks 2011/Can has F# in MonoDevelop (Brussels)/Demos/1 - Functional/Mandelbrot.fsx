#r "/home/tomas/Programs/fsharppowerpack/FSharp.PowerPack.dll"
open System
open Microsoft.FSharp.Math

/// Possible result of evaluation for a single point
type MandelbrotResult = 
    | DidNotEscape
    | Escaped of int


/// Calculates value for a given pixel (complex number)
let mandelbrot c =
    let rec mandelbrotInner (z:Complex) iterations =
        let sq = 
            z.RealPart * z.RealPart + 
            z.ImaginaryPart * z.ImaginaryPart
        if sq >= 4.0 then Escaped iterations
        elif iterations = 100 then DidNotEscape
        else mandelbrotInner ((z * z) + c) (iterations + 1)
    mandelbrotInner c 0


// --------------------------------------------------------
// Draw the mandelbrot fractal to F# Interactive

for y in [-1.0..0.2..1.0] do
    for x in [-2.0..0.1..1.0] do
        match mandelbrot (Complex.Create(x, y)) with
        | DidNotEscape -> Console.Write "#"
        | Escaped it when it > 10 -> Console.Write "x"
        | Escaped it when it > 5 -> Console.Write "+"
        | Escaped it when it > 2 -> Console.Write "."
        | Escaped _ -> Console.Write " "
    Console.WriteLine()

 
  
    
// --------------------------------------------------------
// Draw the mandelbrot fractal using Gtk#

#load "/home/tomas/Programs/fsharp2/bin/load-gtk.fsx"
open Gtk

// Create the user interface
let win = new Window("Mandelbrot Fractal", Visible=true)
let pic = new Gtk.Image(Visible=true)
win.Add(pic)


/// Helper function for creating color
let inline color r g b = 
    (r <<< 16) ||| (g <<< 8) ||| b


/// Draws Mandelbrot fractal on an image
let drawFractal width height = 
    let image = new Gdk.Image(Gdk.ImageType.Normal, Gdk.Visual.System, width, height)
    for y in 0 .. height-1 do
      for x in 0 .. width-1 do
        let xc = float x / float width * 3.0 - 2.0
        let yc = float y / float height * 2.0 - 1.0
        let c = 
            match mandelbrot(Complex.Create(xc, yc)) with
            | DidNotEscape -> 255u
            | Escaped it -> uint32 it
        image.PutPixel(x, y, color 0u c (c / 2u))
    image
    

// Add handler that draws the fractal (if size changes)
pic.ExposeEvent.Add(fun _ ->
  let width, height = pic.Allocation.Width, pic.Allocation.Height
  if pic.ImageProp = null || pic.ImageProp.Width <> width || pic.ImageProp.Height <> height then
    pic.ImageProp <- drawFractal width height)

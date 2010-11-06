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
        elif iterations = 255 then DidNotEscape
        else mandelbrotInner ((z * z) + c) (iterations + 1)
    mandelbrotInner c 0


module Demo.ImagePipeline.Extensions

open System
open System.Drawing
open System.Diagnostics.CodeAnalysis
open Demo.ImagePipeline.Utilities

/// Find luma value for filterTasks corresponding to pixel color
/// by formula recommended by the standard, ITU-R Rec.601
/// see http://en.wikipedia.org/wiki/HSL_and_HSV
let grayLuma (pixel:Color) =
    (int pixel.R * 30 + int pixel.G * 59 + int pixel.B * 11) / 100

let addPixelNoise (rnd:GaussianRandom) (pixel:Color) =
    let next() = rnd.NextInteger()
    let newR = int pixel.R + next()
    let newG = int pixel.G + next()
    let newB = int pixel.B + next()
    let r = max 0 (min 255 newR)
    let g = max 0 (min 255 newG)
    let b = max 0 (min 255 newB)
    Color.FromArgb(r, g, b)

/// Extension methods for System.Drawing.Bitmap
type System.Drawing.Bitmap with 

    /// Creates a grayscale image from a color image
    member source.ToGray() =
        if source = null then nullArg "source"
        let bitmap = new Bitmap(source.Width, source.Height)
        for y in 0 .. bitmap.Height - 1 do
            for x in 0 .. bitmap.Width - 1 do
                let luma = source.GetPixel(x, y) |> grayLuma
                bitmap.SetPixel(x, y, Color.FromArgb(luma, luma, luma))
        bitmap


    /// Creates an image with a border from this image
    member source.AddBorder(borderWidth) =
        if source = null then nullArg "source"
        let bitmap = new Bitmap(source.Width, source.Height)
        for y in 0 .. source.Height - 1 do 
            let yFlag = (y < borderWidth || (source.Height - y) < borderWidth)
            for x in 0 .. source.Width - 1 do
                let xFlag = (x < borderWidth || (source.Width - x) < borderWidth)
                if xFlag || yFlag then
                    let distance = min y (min (source.Height - y) (min x (source.Width - x)))
                    let percent = float distance / float borderWidth
                    let percent2 = percent * percent
                    let pixel = source.GetPixel(x, y)
                    let color = Color.FromArgb(int(float pixel.R * percent2), int(float pixel.G * percent2), int(float pixel.B * percent2))
                    bitmap.SetPixel(x, y, color)
                else
                    bitmap.SetPixel(x, y, source.GetPixel(x, y))
        bitmap

        
    /// Inserts Gaussian noise into a bitmap.
    member source.AddNoise() =
        if source = null then nullArg "source"
        let generator = new GaussianRandom(0.0, 30.0)
        let bitmap = new Bitmap(source.Width, source.Height)
        for y in 0 .. bitmap.Height - 1 do
            for x in 0 .. bitmap.Width - 1 do
                let newPixel = source.GetPixel(x, y) |> addPixelNoise generator
                bitmap.SetPixel(x, y, newPixel)
        bitmap

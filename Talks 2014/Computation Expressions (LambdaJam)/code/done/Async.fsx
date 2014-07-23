#load "lib/WinForms.Async.fsx"
open FSharp.Data
open WinForms.Async

// --------------------------------------------------------
// Working with the Web 
// --------------------------------------------------------

let download() = async { 
  let! req = Http.AsyncRequestString("http://tomsasp.net")
  do! Async.Sleep(1000)
`  printfn "%d" req.Length
}

download () |> Async.Start

// --------------------------------------------------------
// Controlling traffic lights
// --------------------------------------------------------

let lights = TrafficForm() 
lights.DisplayLight Red

let traffic () = async { 
  while true do
    for color in [Red;Green;Orange] do
      lights.DisplayLight color
      do! Async.Sleep(1000) }

traffic() |> Async.StartImmediate

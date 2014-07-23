#load "lib/WinForms.Async.fsx"
open FSharp.Data
open WinForms.Async

// --------------------------------------------------------
// Working with the Web 
// --------------------------------------------------------

let url = "http://www.lambdajam.com/"

let download () = async {
    let! html = Http.AsyncRequestString(url)
    do! Async.Sleep(1000)
    printfn "%d" html.Length
  }

download () |> Async.Start















// --------------------------------------------------------
// Controlling traffic lights
// --------------------------------------------------------

let lights = TrafficForm() 
lights.DisplayLight Green

let run () = async { 
  while true do
    for c in [Green; Orange; Red] do
      do! Async.Sleep(500)
      lights.DisplayLight c
}

run () |> Async.StartImmediate
open System

let rnd = new Random()

let randomPrice max =
  let p1 = Math.Round(rnd.NextDouble() * max, 1)
  let p2 = Math.Round(rnd.NextDouble() * max, 1)
  (p1, p2)

// ------------------------------------------------------------------
// Code for Demos

let normalize range = 
  match range with
  | f, s when f < s -> range
  | f, s -> s, f

let range = randomPrice 100.0
let norm = normalize range

let range2 = randomPrice 100.0 |> normalize
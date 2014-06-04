#r "references/OpenTK.dll"
#r "references/OpenTK.GLControl.dll"
#load "functional3d.fs"

open Functional3D
open System.Drawing

// ------------------------------------------------------------------
// Modelling the puzzle
// ------------------------------------------------------------------

type Color = Black | White
type Kind = Straight | Turn
type Part = Color * Kind

type Position = int * int * int
type Direction = int * int * int

type Shape = list<Part>

// ------------------------------------------------------------------
// Implementing the algorithm
// ------------------------------------------------------------------

/// Given 'Position' and 'Direction' calculate
/// a new position (by adding the offsets)
let move (x,y,z) (dx,dy,dz) = (x+dx, y+dy, z+dz)

/// For a 'Turn' part oriented in the given 'Direction'
/// generate a list of possible next Directions
let offsets (dx, dy, dz) = 
  [ if dx = 0 then for i in [-1;1] do yield i, 0, 0
    if dy = 0 then for i in [-1;1] do yield 0, i, 0
    if dz = 0 then for i in [-1;1] do yield 0, 0, i ]

/// Given a current 'Position' and 'Direction', get a list
/// of possible new Directions and corresponding Positions
let rotate position direction : list<Direction * Position> = 
  [ for offs in offsets direction -> 
      offs, move position offs ]

// ------------------------------------------------------------------
// Checking valid moves
// ------------------------------------------------------------------

/// A set of occupied positions
type CubeState = Set<Position>

/// Expected colors for each position
let colorMap = 
  [ (0,0,0), Black; (0,0,1), White;
    (0,1,0), White; (1,0,0), White;
    (1,1,1), White; (1,1,0), Black;
    (1,0,1), Black; (0,1,1), Black ] 
  |> dict

/// Checks that the specified position is "inside" the
/// cube and there is no part already in that place
let isValidPosition position (state:CubeState) = 
  let x, y, z = position
  let free = not (state.Contains(position))
  let inRange = 
    x >= 0 && y >= 0 && z >= 0 && 
    x <= 3 && y <= 3 && z <= 3
  free && inRange

// ------------------------------------------------------------------
// Generating moves
// ------------------------------------------------------------------

/// Given a current Position & Direction and current
/// Part, get a list of next Positions & Directions
let getPositions position direction part = 
  match part with 
  | (_, Straight) -> [ direction, move position direction ]  
  | (_, Turn) -> rotate position direction

/// Get next valid positions (with directions)
let getValidPositions pos dir part state =
  [ for dir, pos in getPositions pos dir part do
      if isValidPosition pos state then yield dir, pos ]

/// Recursive function that solves the puzzle using backtracking
let rec solve pos dir state trace (shape:Shape) = seq {
  match shape, pos with 
  | [part], _ -> 
      // We have reached the end. Return the trace!
      yield (List.rev trace)
  | part::shape, (x,y,z) when fst part = colorMap.[x/2,y/2,z/2] -> 
      // Current part has the rigth color, get valid 
      // positions for the next part & try all of them
      let moves = getValidPositions pos dir part state 
      for dir, pos in moves do
        let trace = pos::trace
        yield! solve pos dir (Set.add pos state) trace shape 
  | _ -> 
      // Current part does not have the right color
      // (so we have to go back and try another layout)
      () }

// ------------------------------------------------------------------
// Solving the puzzle
// ------------------------------------------------------------------

let puzzle : Shape = 
  // Lookup tables for different colors/kinds
  let clrs  = dict ['b', Black; 'w', White]
  let kinds = dict ['s', Straight; 'r', Turn]
  // Read the string and build a list of 'Part' values
  ( "bbbwwwwwwwwbbbbwwwbbbbbbwwwbbwbbwbbwwwwwbbbbbbwbwwwbbbbwwwwwbbww",
    "srssrrrrrrrrrrsrrssrrrrssrsrrrrrrsrsrrrrrrrrrsrrsrrsrrssrrrsrrss" )
  ||> Seq.map2 (fun clr kind -> clrs.[clr], kinds.[kind])
  |> List.ofSeq

// Pick starting location
let start = (0, 0, 0)
// Run the 'solve' function 
let res = solve start (0, 0, 1) (set [start]) [start] puzzle 

// Pick the first solution & print positions
let solution = Seq.head res 
solution |> Seq.iteri (fun i p -> 
  printfn "%d - %A" (i+1) p)

// ------------------------------------------------------------------
// Building 3D visualization
// ------------------------------------------------------------------

/// Convert coordinate to float values
let fl (x,y,z) = (float x, float y, float z)

/// Draw the first 'i' steps of the puzzle
let draw i =
  solution 
  |> Seq.take i
  |> Seq.map (fun ((x, y, z) as p) -> 
      // Get the expected color based on the color map
      let color = 
        if colorMap.[x/2,y/2,z/2] = Black then 
          Color.SaddleBrown else Color.BurlyWood
      // Create a coloured small cube & move it 
      Fun.cube
      |> Fun.scale (0.95,0.95,0.95)
      |> Fun.color color
      |> Fun.translate (fl p) )
  |> Seq.reduce ($)


Async.StartImmediate <| async { 
  for i in 1 .. 64 do 
    do! Async.Sleep(200)
    Fun.show (draw i) }

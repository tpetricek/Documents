// Declaring our own simple list

type IntList = 
  | Empty
  | Cons of int * IntList

let ints = Cons(1, Cons(2, Cons(3, Cons(4, Empty))))

// Working with standard F# list

let nums = [ ] // [ 1 .. 10 ]

match nums with 
| []         -> printfn "Empty list"
| head::tail -> printfn "Starting with %d" head

// ------------------------------------------------------------------
// TODO: Write this code

let rec sumList list =
  match list with
  | []         -> 0
  | head::tail -> head + sumList tail

let rec reduceList zero op list =
  match list with
  | []         -> zero
  | head::tail -> op head (reduceList zero op tail)

reduceList 1 (*) nums
reduceList System.Int32.MinValue max nums
reduceList "" (fun n s -> sprintf "%d, %s" n s) nums
reduceList "" (sprintf "%d, %s") nums
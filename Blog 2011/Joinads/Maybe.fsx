#nowarn "26"
// --------------------------------------------------------------------------------------

type MaybeBuilder() =
  // Standard monadic 'bind', 'return' and 'zero'
  member x.Bind(v, f) = Option.bind f v
  member x.Return(a) = Some a
  member x.ReturnFrom(o) = o
  member x.Fail() = None

  // Combine two options into option of tuple 
  member x.Merge(v1, v2) = 
    match v1, v2 with
    | Some a, Some b -> Some (a, b)
    | _ -> None
  // Return first option that contains value
  member x.Choose(v1, v2) = 
    match v1 with 
    | Some(v1) -> Some(v1) 
    | _ -> v2

  // Creating & executing delayed computations
  member x.Delay(f) = f
  member x.Run(f) = f()

// Create an instance of the computation builder
let maybe = new MaybeBuilder()

// --------------------------------------------------------------------------------------

/// Logical 'or' operator for ternary (Kleene) logic
let kleeneOr a b = maybe {
  match! a, b with
  | !true, _ -> return true
  | _, !true -> return true 
  | !a, !b -> return a || b }

// Print truth table for the ternary operator
for a in [Some true; None; Some false] do
  for b in [Some true; None; Some false] do
    printfn "%A or %A = %A" a b (kleeneOr a b)

// --------------------------------------------------------------------------------------

let a = Some true
let b = Some true

// Translation of individual clauses - inputs are combined 
// using 'Merge' and body is wrapped using 'Delay'
let cl1 = maybe.Bind(a, function 
  | true -> maybe.Return(maybe.Delay(fun () -> maybe.Return(true)))
  | _ -> maybe.Fail() )
let cl2 = maybe.Bind(b, function 
  | true -> maybe.Return(maybe.Delay(fun () -> maybe.Return(true)))
  | _ -> maybe.Fail() )
let cl3 = maybe.Bind(maybe.Merge(a, b), fun (a, b) -> 
  maybe.Return(maybe.Delay(fun () -> maybe.Return(true))))

// Clauses are combined using 'Choose' and selected
// delayed clause is then evaluated using 'Run'
maybe.Bind(maybe.Choose(maybe.Choose(cl1, cl2), cl3), fun r -> 
  maybe.Run(r))

// --------------------------------------------------------------------------------------

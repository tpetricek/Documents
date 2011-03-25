#nowarn "26"
open System
open System.Threading
open System.Threading.Tasks

// --------------------------------------------------------------------------------------
// Computation for Futures
// --------------------------------------------------------------------------------------

module Futures = 

  // Implementation of joinad operations for working with tasks

  let run f = f()
  let delay f = f
  let unit v = Task.Factory.StartNew(fun () -> v)
  let bind f (v:Task<_>) = 
    v.ContinueWith(fun (v:Task<_>) ->
      let task : Task<_> = f(v.Result)
      task).Unwrap()
  let merge t1 t2 = 
    t1 |> bind (fun v1 ->
      t2 |> bind (fun v2 -> unit (v1, v2)))
  let choose (t1:Task<'a>) (t2:Task<'a>) : Task<'a> = 
    let tok = new CancellationTokenSource()
    Task.Factory.ContinueWhenAny
      ( [| t1 :> Task; t2 :> Task |],
        (fun (t:Task) -> 
            let res = (t :?> Task<'a>).Result
            tok.Cancel()
            res ),
        tok.Token)
  let fail () = 
    let never = 
      { new IAsyncResult with
          member x.AsyncState = null
          member x.AsyncWaitHandle = new AutoResetEvent(false) :> WaitHandle
          member x.CompletedSynchronously = false
          member x.IsCompleted = false }
    Task.Factory.FromAsync(never, fun _ -> failwith "!") 

  // Definition of the computation builder

  type FutureBuilder() =
    member x.Return(a) = unit a
    member x.Bind(v, f) = bind f v
    member x.Merge(f1, f2) = merge f1 f2
    member x.Choose(f1, f2) = choose f1 f2
    member x.Fail() = fail()
    member x.Delay(f) = delay f
    member x.Run(t) = run t

  let future = new FutureBuilder()

// --------------------------------------------------------------------------------------
/// Examples of working with futures
// --------------------------------------------------------------------------------------

module FutureTests = 
  open Futures

  // Helper function for creating simple delayed tasks
  let after t v = 
    Task.Factory.StartNew(fun () -> 
      Thread.Sleep(t:int)
      v)

  // Using applicative style for running two tasks in parallel
  let f1 = future {
    let! a = after 2000 1 
    and b = after 2000 2
    return a + b }

  printfn "Result: %A" f1.Result

  // Parallel OR 
  let f2 = future {
    match! after 10000 true, after 100 true with
    | !true, _ -> return true
    | _, !true -> return true
    | !b1, !b2 -> return b1 || b2 }

  printfn "Result: %b" f2.Result


  // Classical joinads example - multiplying with shortcircuiting
  // (Note: side-effects are only executed for the selected clause)
  let f3 = future {
    match! after 2000 10, after 500 0 with
    | !0, _  -> printfn "(in 1)"
                return "t1 returned 0"
    | _, !0  -> printfn "(in 2)"
                return "t2 returned 0"
    | !a, !b -> printfn "(in 3)"
                return sprintf "multiplied: %d" (a * b) }
  
  printfn "Result: %A" f3.Result

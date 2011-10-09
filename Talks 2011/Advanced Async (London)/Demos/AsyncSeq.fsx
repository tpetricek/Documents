#r "FSharp.AsyncExtensions.dll"
open FSharp.Control

// ----------------------------------------------------------------------------
// Introduction asynchronous sequences - asynchronous sequences allow
// both asynchronous waiting using 'let!' and returning multiple
// elements using 'yield'. They are evaluated on demand, which means
// that the consumer has control over the evaluation.

let numbers = asyncSeq {
  do! Async.Sleep(200)
  printfn "Numbers: yielding 1"
  yield 1

  do! Async.Sleep(200)
  printfn "Numbers: yielding 2"
  yield 2

  do! Async.Sleep(200)
  printfn "Numbers: yielding 3"
  yield 3 }

// ----------------------------------------------------------------------------

let rec iterate input = 
  async {
    let! s = input
    match s with
    | AsyncSeqInner.Nil -> 
        printfn "Iterate: end"
    | AsyncSeqInner.Cons(v, rest) ->
        printfn "Iterate: got %d" v 
        return! iterate rest }

iterate numbers |> Async.Start

// ----------------------------------------------------------------------------

async {
  for n in numbers do 
    do! Async.Sleep(1000)
    printfn "Iterate: got %d" n
  printfn "Iterate: end" }
|> Async.Start

#nowarn "40"
#r "..\\bin\\FSharp.AgentExtensions.dll"

open FSharp.Control
open System.Collections.Generic

type BufferMessage<'T> = 
  | Put of 'T
  | Get of AsyncReplyChannel<'T>

let agent = Agent<BufferMessage<int>>.Start(fun agent -> 
  let queue = new Queue<_>()
  let rec nonempty = async {
    let! msg = agent.Receive()
    match msg with 
    | Put v -> 
        queue.Enqueue(v)
        return! nonempty 
    | Get reply ->
        reply.Reply(queue.Dequeue())
        if queue.Count = 0 then return! empty 
        else return! nonempty }
  and empty = agent.Scan(function
    | Put v -> Some <| async {
        queue.Enqueue(v)
        return! nonempty }
    | _ -> None)
  empty )
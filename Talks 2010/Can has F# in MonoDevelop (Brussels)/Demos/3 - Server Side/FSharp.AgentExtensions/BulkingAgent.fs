// ----------------------------------------------------------------------------
// Bulking agent
// ----------------------------------------------------------------------------
namespace FSharp.Control

open System
open System.Collections.Generic
open FSharp.Control

/// Agent that implements bulking of incomming messages
/// A bulk is produced (using an event) either after enough incomming
/// mesages are collected or after the specified time (whichever occurs first)
type BulkingAgent<'T>(bulkSize, timeout) = 

  // Used to report the aggregated bulks to the user
  let bulkEvent = new Event<'T[]>()

  let agent : Agent<'T> = Agent.Start(fun agent -> 

    // Represents the control loop of the agent
    // - start  The time when last bulk was processed
    // - list   List of items in current bulk
    let rec loop (start:DateTime) (list:_ list) = async {
      if (DateTime.Now - start).TotalMilliseconds > float timeout then
        // Timed-out - report bulk if there is some message & reset time
        if list.Length > 1 then 
          bulkEvent.Trigger(list |> Array.ofList)
        return! loop DateTime.Now []
      else
        // Try waiting for a message
        let! msg = agent.TryReceive(timeout = timeout / 25)
        match msg with 
        | Some(msg) when list.Length + 1 = bulkSize ->
            // Bulk is full - report it to the user
            bulkEvent.Trigger(msg :: list |> Array.ofList)
            return! loop DateTime.Now []
        | Some(msg) ->
            // Continue collecting more mesages
            return! loop start (msg::list)
        | None -> 
            // Nothing received - check time & retry
            return! loop start list }
    loop DateTime.Now [] )

  member x.BulkProduced = bulkEvent.Publish
  member x.Enqueue v = agent.Post(v)

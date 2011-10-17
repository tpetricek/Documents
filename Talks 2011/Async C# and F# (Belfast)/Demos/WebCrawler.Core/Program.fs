// ----------------------------------------------------------------------------
// Implements simple agent that can be used for storing 
// the state of crawling in F# using 'MailboxProcessor'
// ----------------------------------------------------------------------------
namespace WebCrawler.Core

// Agent can accept two types of messages - one is to enqueue
// more work items (and mark a URL as visited) and the other 
// is to get the next work item
type private CrawlerMessage =
  | EnqueueWorkItems of (string * string[])
  | GetWorkItem of AsyncReplyChannel<string>


/// The agent can be used to store state of crawling. The caller can use 
/// 'GetAsync' method to get the next work item from the queue and it can 
/// report result using 'Enqueue' (this will mark URL as visited and add
/// more URLs to the working queue).
type CrawlerAgent() =
  let agent = MailboxProcessor.Start(fun agent ->

    /// Add processed URL to the 'visited' set and then add all 
    /// new pending URLs and filter those that were visited
    let rec addItems visited pending (from, work) = 
      let visited = Set.add from visited
      let newWork = 
        List.append pending (List.ofSeq work)
        |> List.filter (fun v -> 
        not (Set.contains v visited))
      nonEmpty visited newWork

    /// Represents state when the pending queue contains some URLs
    and nonEmpty visited pending = async {
      let! msg = agent.Receive()
      match msg with
      | EnqueueWorkItems(res) -> 
          // We can add more items to the queue
          return! addItems visited pending res
      | GetWorkItem(repl) -> 
          // There are some pending items, so we can send one back
          match pending with 
          | first::rest ->
              repl.Reply(first)
              if rest = [] then return! empty visited
              else return! nonEmpty visited rest
          | _ -> failwith "unexpected" }

    /// Represents state when the pending queue is emtpy
    and empty visited = 
      agent.Scan(function
        // We can add more items to the queue
        | EnqueueWorkItems(res) ->
            Some(addItems visited [] res)
        // We cannot process 'GetWorkItem' message in this state
        | _ -> None)

    empty Set.empty)

  /// Start the agent by adding the specified URL to work items        
  member x.Start(url) =
    agent.Post(EnqueueWorkItems("n/a", [| url |]))
  /// Enqueue results of crawling and mark 'from' url as visited
  member x.Enqueue(from, urls) =
    agent.Post(EnqueueWorkItems(from, urls))
  /// Asynchronously get the next work item (returns Task that can be used from C#)
  member x.GetAsync() =
    agent.PostAndAsyncReply(GetWorkItem) 
    |> Async.StartAsTask
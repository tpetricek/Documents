// ----------------------------------------------------------------------------
// Blocking queue agent
// ----------------------------------------------------------------------------
namespace AsyncAgents

open System
open System.Threading.Tasks
open System.Collections.Generic

type Agent<'T> = MailboxProcessor<'T>

type internal BlockingAgentMessage<'T> = 
  | Add of 'T * AsyncReplyChannel<unit> 
  | Get of AsyncReplyChannel<'T>

/// Agent that implements an asynchronous blocking queue
type BlockingQueueAgent<'T>(maxLength) =
  let agent = Agent.Start(fun agent ->
    
    let queue = new Queue<_>()

    let rec emptyQueue() = 
      agent.Scan(fun msg ->
        match msg with 
        | Add(value, reply) -> Some(enqueueAndContinue(value, reply))
        | _ -> None )
    and fullQueue() = 
      agent.Scan(fun msg ->
        match msg with 
        | Get(reply) -> Some(dequeueAndContinue(reply))
        | _ -> None )
    and runningQueue() = async {
      let! msg = agent.Receive()
      match msg with 
      | Add(value, reply) -> return! enqueueAndContinue(value, reply)
      | Get(reply) -> return! dequeueAndContinue(reply) }

    and enqueueAndContinue (value, reply) = async {
      reply.Reply() 
      queue.Enqueue(value)
      return! chooseState() }
    and dequeueAndContinue (reply) = async {
      reply.Reply(queue.Dequeue())
      return! chooseState() }
    and chooseState() = 
      if queue.Count = 0 then emptyQueue()
      elif queue.Count < maxLength then runningQueue()
      else fullQueue()

    // Start with an empty queue
    emptyQueue() )

  new () = BlockingQueueAgent(Int32.MaxValue)

  /// Asynchronously adds item to the queue. The operation ends when
  /// there is a place for the item. If the queue is full, the operation
  /// will block until some items are removed.
  member x.AsyncAdd(v:'T, ?timeout) = 
    agent.PostAndAsyncReply((fun ch -> Add(v, ch)), ?timeout=timeout)

  /// Asynchronously gets item from the queue. If there are no items
  /// in the queue, the operation will block unitl items are added.
  member x.AsyncTake(?timeout) = 
    agent.PostAndAsyncReply(Get, ?timeout=timeout)


  /// Asynchronously adds item to the queue. The operation ends when
  /// there is a place for the item. If the queue is full, the operation
  /// will block until some items are removed. The method returns 
  /// started Task that can be used from C#.
  member x.AddAsync(v:'T) =
    (agent.PostAndAsyncReply(fun ch -> Add(v, ch))
    |> Async.StartAsTask) :> Task

  /// Asynchronously gets item from the queue. If there are no items
  /// in the queue, the operation will block unitl items are added.
  /// The method returns started Task that can be used from C#.
  member x.TakeAsync() = 
    agent.PostAndAsyncReply(Get)
    |> Async.StartAsTask
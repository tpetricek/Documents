namespace Pit.Async

open Pit
open Pit.Dom
open Pit.Javascript

// --------------------------------------------------------------------------------------

type Async<'T> = Cont of (('T -> unit) -> unit)

type AsyncBuilder() =
  [<Js>] 
  member x.Bind(Cont v, f) = 
    Cont(fun k -> v (fun a -> let (Cont r) = f a in r k))      
  [<Js>] 
  member x.Delay(f) = Cont (fun k -> let (Cont r) = f () in r k)
  [<Js>] 
  member x.Zero () = Cont (fun k -> k())
  [<Js>] 
  member x.ReturnFrom(w) :Async<'T> = w
  [<Js>] 
  member x.Return(v) :Async<'T> = Cont(fun k -> k v)
  [<Js>] 
  member this.While(cond, body) = 
    let x = this
    let rec loop() = 
      if cond() then x.Bind(body, loop)
      else x.Zero()
    loop()

[<AutoOpen>]
module AsyncTopLevel = 

  [<Js>]
  let async = AsyncBuilder()

  type Pit.Javascript.XMLHttpRequest with
    [<Js>]
    member x.AsyncSend() = Cont(fun k ->
      x.OnReadyStateChange <- (fun () -> 
        if x.ReadyState = 4 then
          k x.ResponseText)
      x.Send(""))

  type Async =
    [<Js>] 
    static member StartImmediate(workflow:Async<unit>) =
      let (Cont f) = workflow
      f (fun () -> ())

    [<Js>] 
    static member Sleep(milliseconds:int) = Cont(fun k ->
      window.SetTimeout((fun () -> k ()), milliseconds) |> ignore)

    [<Js>] 
    static member AwaitEvent(event:IEvent<'T>) = Cont(fun k ->
      let hndl = ref null
      hndl := Handler(fun o _ ->
        let e = o :?> 'T
        event.RemoveHandler(!hndl)
        k e )
      event.AddHandler(!hndl) )

    [<Js; CompiledName("AwaitEvent2")>] 
    static member AwaitEvent(event1:IEvent<'T1>, event2:IEvent<'T2>) = Cont(fun k ->
      let event = Event.merge (Event.map Choice1Of2 event1) (Event.map Choice2Of2 event2)
      let hndl = ref null
      hndl := Handler(fun _ e ->
        event.RemoveHandler(!hndl)
        k e )
      event.AddHandler(!hndl) )


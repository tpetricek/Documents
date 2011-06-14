#nowarn "40"
namespace FSharp.CheckedAsync

open System
open System.Windows

type FsAsync = Microsoft.FSharp.Control.Async

// ----------------------------------------------------------------------------

/// Represents an asynchronous computation that starts
/// on the 'Pre thread, ends on the 'Post thread and
/// produces a value of type 'Value.
type CheckedAsync<'Pre, 'Post, 'Value> = SA of Async<'Value>

/// Phantom type representing computations running on GUI thread
type GuiThread = interface end
/// Phantom type representing computations running on a background thread
type BackgroundThread = interface end
/// Phantom type representing computations running on any (unknown thread)
type AnyThread = interface end


/// Synchronization context wrapper that keeps a phantom type 
/// representing the kind of the thread in the type
type SynchronizationContext<'T>(ctx:Threading.SynchronizationContext) = 
  member x.Context = ctx

// ----------------------------------------------------------------------------

module Utils = 

  /// Calls the continuation on the same thread where it was started
  let synchronize f = 
    let ctx = System.Threading.SynchronizationContext.Current 
    f (fun g arg ->
      let nctx = System.Threading.SynchronizationContext.Current 
      if ctx <> null && ctx <> nctx then ctx.Post((fun _ -> g(arg)), null)
      else g(arg) )

module AsyncExtensions = 
  open Utils

  /// Waits for an occurrence of an event asynchronously
  type Microsoft.FSharp.Control.Async with 
    static member AwaitObservable(ev1:IObservable<'a>) : Async<_> =
      synchronize (fun f ->
        Async.FromContinuations((fun (cont,econt,ccont) -> 
          let rec callback = (fun value ->
            remover.Dispose()
            f cont value )
          and remover : IDisposable  = ev1.Subscribe(callback) 
          () )))

// ----------------------------------------------------------------------------

/// Wrappers for standard operations provided by F# 'Async' type
type CheckedAsync =
  static member Start(SA a:CheckedAsync<BackgroundThread, 'Post, unit>) = 
    let disp = System.Windows.Deployment.Current.Dispatcher
    FsAsync.Start(a)

  static member StartImmediate(SA a:CheckedAsync<GuiThread, 'Post, unit>) = 
    FsAsync.StartImmediate(a)

  static member StartChild(SA a:CheckedAsync<BackgroundThread, 'Post, 'R>) 
      : CheckedAsync<'Pre1, 'Pre1, CheckedAsync<'Pre2, 'Pre2, 'R>> = 
    SA(async { let! res = FsAsync.StartChild(a)
               return SA res })

  static member SwitchToGui() : CheckedAsync<'Pre, GuiThread, unit> = 
    let disp = System.Windows.Deployment.Current.Dispatcher
    SA(FsAsync.FromContinuations(fun (cont, _, _) ->
      disp.BeginInvoke(fun () -> 
        cont()) |> ignore))

  static member SwitchToThreadPool() : CheckedAsync<'Pre, BackgroundThread, unit> = 
    SA(FsAsync.SwitchToThreadPool())

  static member SwitchToContext(ctx:SynchronizationContext<'C>) : CheckedAsync<'Pre, 'C, unit> = 
    SA(FsAsync.SwitchToContext(ctx.Context))

  static member CurrentContext() : CheckedAsync<'T, 'T, SynchronizationContext<'T>> =
    SA(async.Return(new SynchronizationContext<'T>(Threading.SynchronizationContext.Current)))

  static member UnsafeSwitchToAnything() : CheckedAsync<_, _, _> = 
    async.Zero() |> SA

// ----------------------------------------------------------------------------
// Extensions for some of the standard .NET types providing async API

[<AutoOpen>]
module Extensions = 
  open Utils

  type System.Net.WebClient with
    member x.AsyncDownloadString(address) : CheckedAsync<'T, 'T, string> =
      Async.FromContinuations (fun (cont, econt, ccont) ->
        let rec disp : IDisposable = x.DownloadStringCompleted.Subscribe(fun (args:Net.DownloadStringCompletedEventArgs) ->
          disp.Dispose()
          if args.Cancelled then ccont (new OperationCanceledException("")) 
          elif args.Error <> null then econt args.Error
          else cont args.Result)
        x.DownloadStringAsync(address, null)) |> SA

  type CheckedAsync with 
    static member AwaitObservable(ev1:IObservable<'a>) : CheckedAsync<GuiThread, GuiThread, _> =
      synchronize (fun f ->
        SA(Async.FromContinuations((fun (cont,econt,ccont) -> 
          let rec callback = (fun value ->
            remover.Dispose()
            f cont value )
          and remover : IDisposable  = ev1.Subscribe(callback) 
          () ))))

// ----------------------------------------------------------------------------
// Computation builder for checked async (that keeps track of threads)

type CheckedAsyncBuilder<'TStart>() =
  member x.Bind(SA a:CheckedAsync<'Pre, 'Interm, 'V1>, f:'V1 -> CheckedAsync<'Interm, 'Post, 'V2>) : CheckedAsync<'Pre, 'Post, 'V2> =
    SA(async.Bind(a, fun v -> let (SA b) = f v in b))
  member x.Combine(SA a:CheckedAsync<'Pre, 'Interm, unit>, SA b:CheckedAsync<'Interm, 'Post, 'V>) : CheckedAsync<'Pre, 'Post, 'V> =
    SA(async.Combine(a, b))
  member x.Delay(f:unit -> CheckedAsync<'Pre, 'Post, 'V>) : CheckedAsync<'Pre, 'Post, 'V> =
    SA(async.Delay(fun () -> let (SA a) = f() in a))
  member x.For(seq:seq<_>, body : _ -> CheckedAsync<'Thread, 'Thread, _>) : CheckedAsync<'Thread, 'Thread, _> = 
    SA(async.For(seq, fun v -> let (SA a) = body v in a))
  member x.Return(v:'V) : CheckedAsync<'Cond, 'Cond, 'V> = 
    SA(async.Return(v)) 
  member x.ReturnFrom(SA v:CheckedAsync<'T, 'T, 'V>) : CheckedAsync<'T, 'T, 'V> = 
    SA(async.ReturnFrom(v)) 
  member x.TryFinally(SA computation:CheckedAsync<'Pre, 'Post, _>, compensation) : CheckedAsync<'Pre, 'Post, _> =
    SA(async.TryFinally(computation, compensation))
  member x.TryWith(SA computation:CheckedAsync<'Pre, 'Post, _>, handler:_ -> CheckedAsync<AnyThread, 'Post, _>) : CheckedAsync<'Pre, 'Post, _> =
    SA(async.TryWith(computation, (fun e -> let (SA h) = handler e in h)))
  member x.Using(a:#IDisposable, f:'T -> CheckedAsync<'Pre, 'Post, 'R>) : CheckedAsync<'Pre, 'Post, 'R> =
    SA(async.Using(a, fun v -> let (SA b) = f v in b))
  member x.While(f, SA body : CheckedAsync<'Thread, 'Thread, _>) : CheckedAsync<'Thread, 'Thread, _> = 
    SA(async.While(f, body))
  member x.Zero() : CheckedAsync<'Cond, 'Cond, unit>  = 
    SA(async.Zero())
  member x.Run(op:CheckedAsync<'TStart, _, _>) = op

// ----------------------------------------------------------------------------

[<AutoOpen>]
module Values =
  /// Creates a checked asynchronous workflow running on the specified thread
  let asyncOn (phantom:'T) = CheckedAsyncBuilder<'T>()
  
  /// Creates a checked asynchronous workflow and infers its thread
  let asyncChecked<'T> = CheckedAsyncBuilder<'T>()

  /// Dummy value for use with 'asyncOn'
  let gui = { new GuiThread }
  /// Dummy value for use with 'asyncOn'
  let background = { new BackgroundThread }

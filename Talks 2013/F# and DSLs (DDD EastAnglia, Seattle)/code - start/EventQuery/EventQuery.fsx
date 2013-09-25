open System
open System.Net

/// Asynchronously downloads stock prices from Yahoo
let downloadPrices stock = 
  let evt = Event<_>()
  let work = 
    async {
      // Download price from Yahoo
      let wc = new WebClient()
      let url = "http://ichart.finance.yahoo.com/table.csv?s=" + stock
      let! html = wc.AsyncDownloadString(Uri(url)) 
      let lines = html.Split([|'\n'; '\r'|], StringSplitOptions.RemoveEmptyEntries)
      let lines = lines |> Seq.skip 1 |> Array.ofSeq |> Array.rev

      // Return sequence that reads the prices
      while true do
        for line in lines do
          let infos = (line:string).Split(',')
          let dt = DateTime.Parse(infos.[0])
          let op = float infos.[1] 
          do! Async.Sleep(100)
          printfn "%A: %f" dt op
          evt.Trigger(dt, op) }
  let cts = new System.Threading.CancellationTokenSource()
  Async.StartImmediate(work, cts.Token)
  cts, evt.Publish


type Nop = Nop

type EventBuilder() = 
  member x.For(event:IEvent<_, _>, f:'T -> Nop) : IEvent<'T> =  
    event |> Event.map id 
  member x.Yield(v:'T) = Nop

  [<CustomOperation("select", MaintainsVariableSpace = false, AllowIntoPattern = true)>]
  member x.Select(event, [<ProjectionParameter>] f) =
    event |> Event.map f

  [<CustomOperation("where", MaintainsVariableSpace = true, AllowIntoPattern = false)>]
  member x.Where(event, [<ProjectionParameter>] f) =
    event |> Event.filter f

  [<CustomOperation("pairwise", MaintainsVariableSpace = false, AllowIntoPattern = true)>]
  member x.Pairwise(event) =
    event |> Event.pairwise 

  [<CustomOperation("iter", MaintainsVariableSpace = false, AllowIntoPattern = false)>]
  member x.Iter(event, [<ProjectionParameter>] f) =
    event |> Event.add f




// --------------------------------------------------------------------------------------

let event = EventBuilder()

open System.Windows.Forms
let frm = new Form(Visible = true)

event { for e in frm.MouseDown do
        iter (printfn "%A" e) }

event { for e in frm.MouseDown do
        pairwise into (e1, e2)
        select (e1.X - e2.X, e1.Y - e2.Y) into r
        iter (printfn "%A" r) }








let cancel, evt = downloadPrices "MSFT"

event { for e in evt do
        pairwise into (e1, e2) 
        where (abs ((snd e1) - (snd e2)) > 1.0)
        select (fst e1) into r
        iter (printfn "!!!!!!!  %A   !!!!!!!" r) }

cancel.Cancel()
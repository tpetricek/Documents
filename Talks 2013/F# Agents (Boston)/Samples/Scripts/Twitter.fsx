#nowarn "40"
#r "../lib/FSharp.Data.dll"
#load "../lib/FSharpChart.fsx"
#load "../lib/GuiExtensions.fs"
#load "../lib/Twitter.fs"
#load "../lib/BatchProcessing.fs"

open System
open System.Threading
open System.Windows.Forms
open System.Collections.Generic
open MSDN.FSharp.Charting

open FSharp.Control
open FSharp.WebBrowser
open FSharp.TwitterAPI
open FSharp.TimeSeries

// ----------------------------------------------------------------------------
// Create user interface and connect to Twitter
// ----------------------------------------------------------------------------

let frm = new Form(TopMost = true, Visible = true, Width = 500, Height = 400)
let btn = new Button(Text = "Pause", Dock = DockStyle.Top)
let web = new WebBrowser(Dock = DockStyle.Fill)
frm.Controls.Add(web)
frm.Controls.Add(btn)

let key = "CoqmPIJ553Tuwe2eQgfKA"
let secret = "dhaad3d7DreAFBPawEIbzesS1F232FnDsuWWwRTUg"
let connector = Twitter.authenticate (key, secret) web.Navigate

// NOTE: Run all code up to this point. A window should appear. You can then
// login to twitter and you'll get a pin code that you need to copy and
// paste as an argument to the 'Connect' method below:
let twitter = connector.Connect("5221387")

// Login: 'fsharpd'
// Password: 'fsharp123'

// ----------------------------------------------------------------------------
// Using Twitter APIs
// ----------------------------------------------------------------------------

// Download the current home timeline
web.Output.StartList()

let home = Twitter.homeTimeline twitter
for item in home do
  web.Output.AddItem "<strong>%s</strong>: %s" item.User.Name item.Text
 

// Display stream with live data
web.Output.StartList()

let sample = Twitter.sampleTweets twitter
sample.TweetReceived |> Observable.guiSubscribe (fun status ->
    match status.Text, status.User with
    | Some text, Some user ->
        web.Output.AddItem "<strong>%s</strong>: %s" user.Name text
    | _ -> ()  )
sample.Start()
sample.Stop()


// ----------------------------------------------------------------------------
// Pausing the twitter stream using agents
// ----------------------------------------------------------------------------

// Create an agent that allows pausing the stream
let tweetProduced = Event<string * string>()

type PauserMessage = 
  | Pause 
  | Resume
  | Post of (string * string)

let pauser = Agent.Start(fun inbox -> 

  let rec running () = async {
    let! msg = inbox.Receive()
    match msg with 
    | Pause -> return! paused ()
    | Resume -> return! running ()
    | Post(user, msg) -> 
        tweetProduced.Trigger(user, msg) 
        return! running() }
  and paused () = inbox.Scan(function
    | Resume -> Some(running ()) 
    | _ -> None) 
    
  running () )

// TODO: Define message type with Pause & Resume & Post

// Display stream with live data
web.Output.StartList()

let sample3 = Twitter.sampleTweets twitter
sample3.TweetReceived |> Observable.guiSubscribe (fun status ->
    try 
      pauser.Post(Post(status.User.Value.Name, status.Text.Value))
    with _ -> ()  )
  

// Register handler for the Pause button
btn.Click |> Observable.subscribe (fun _ ->
  match btn.Text with
  | "Pause" -> 
      btn.Text <- "Resume"
      pauser.Post(Pause)
  | _ ->       
      btn.Text <- "Pause" 
      pauser.Post(Resume) )

// Subscribe to the event, triggering on the GUI
tweetProduced.Publish |> Observable.guiSubscribe (fun (name, text) ->
  web.Output.AddItem "<strong>%s</strong>: %s" name text)

sample3.Start()
sample3.Stop()


// ----------------------------------------------------------------------------
// Analyze words
// ----------------------------------------------------------------------------


// Analyzing text and displaying a chart
let groupWords strings first = 
  let data =
    [ for (s:string) in strings do
        if s <> null then
          yield! s.Split([| ' '; '\t'; '\r'; '\n'; ':' |], StringSplitOptions.RemoveEmptyEntries) ]
    |> Seq.groupBy id
    |> Seq.map (fun (k, v) -> k, Seq.length v)
    |> Seq.sortBy (fun (k, v) -> -v)
    |> List.ofSeq
  if data.Length = 0 then [ "empty", 1 ]
  elif data.Length < first then data 
  else data |> Seq.take first |> List.ofSeq

let chart =
  groupWords ["test1 test1 test2" ] 4
  |> FSharpChart.Column
  |> FSharpChart.Create
 
chart.SetData(groupWords ["hi hi there hi there a b c d"; "hi a b c"; "hi d b b" ] 4)

// Creating an agent that stores data in a ring buffer
type BufferMessage<'T> = 
  | Add of 'T
  | Get of AsyncReplyChannel<'T[]>

let agent = Agent<BufferMessage<string>>.Start(fun inbox -> async {
  // TODO: Create an instance of RingBuffer storing 100 past values
  let ring = Array.zeroCreate(100)
  let next = 
    let index = ref 0
    fun () -> index := (!index + 1) % 100; !index
  while true do
    let! msg = inbox.Receive()
    match msg with
    | Add str ->
        ring.[next()] <- str
    | Get repl ->
        repl.Reply(ring) })

    
// Read live data and post it to the agent
web.Output.StartList()

let sample2 = Twitter.sampleTweets twitter
sample2.TweetReceived |> Observable.guiSubscribe (fun status ->
    try agent.Post(Add status.Text.Value)
    with _ -> ()  )

// Get live data from the buffer and update the chart
let cts = new CancellationTokenSource()

let loop = async {
  while true do
    do! Async.Sleep(1000)
    // TODO: Use 'PostAndAsyncReply' with 'Get' to obtain current values
    let! res = agent.PostAndAsyncReply(Get)
    chart.SetData(groupWords res 6) }

Async.StartImmediate(loop, cts.Token)
cts.Cancel()

sample2.Start()
sample2.Stop()


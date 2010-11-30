#nowarn "40"
#r "..\\bin\\FSharp.AgentExtensions.dll"

open System.Threading
open System.Windows.Forms
open System.Collections.Generic

open FSharp.Control
open FSharp.WebBrowser
open FSharp.TwitterAPI

// ----------------------------------------------------------------------------
// Create user interface and connect to Twitter

let frm = new Form(TopMost = true, Visible = true, Width = 400)
let btn = new Button(Text = "Pause", Dock = DockStyle.Top)
let web = new WebBrowser(Dock = DockStyle.Fill)
frm.Controls.Add(web)
frm.Controls.Add(btn)

let key = "OkNRG3fmc6gQnGdRXU0AA"
let secret = "kaAOWpWXUkckyg1cvg9dZK3Lyi7gW227hXhgPosUU"
let connection = TwitterConnector.Authenticate(key, secret, web.Navigate) 
let twitter = connection.Connect("0656785")


 


// ----------------------------------------------------------------------------
// Display all Tweets as they come...

web.Output.StartList()

twitter.TweetReceived |> Observable.guiSubscribe (fun status ->
  web.Output.AddItem "<strong>%s</strong>: %s" status.UserName status.Status )

twitter.StartSearch [ "fsharp" ]
twitter.Cancel()








// ----------------------------------------------------------------------------
// Pause the twitter stream and cache all statuses

let status = new Event<UserStatus>()

type TwitterBufferMessage = 
  | Pause
  | Resume
  | Status of UserStatus

let agent = Agent<_>.Start(fun agent -> 
  // When running, we accept all messages
  let rec running = async {
    let! msg = agent.Receive()
    match msg with 
    | Status tweet -> 
        status.Trigger(tweet)
        return! running
    | Resume -> return! running
    | Pause  -> return! paused } 
  
  // When paused, we wait for the 'Resume' message
  and paused = agent.Scan(function
    | Resume -> Some(running)
    | Pause -> Some(paused)
    | _ -> None )
  
  // Agent is initially running
  running )


// Register event handlers
status.Publish |> Observable.guiSubscribe (fun status ->
  web.Output.AddItem "<strong>%s</strong>: %s" status.UserName status.Status )

btn.Click |> Observable.subscribe (fun _ ->
  match btn.Text with
  | "Pause" -> btn.Text <- "Resume"
               agent.Post(Pause)
  | _ ->       btn.Text <- "Pause" 
               agent.Post(Resume) )

// Start search
web.Output.StartList()

twitter.TweetReceived |> Observable.subscribe (Status >> agent.Post)
twitter.StartSearch [ "mercury" ]

twitter.Cancel()





// ----------------------------------------------------------------------------
// Bulk processing

web.Output.StartList()

let bulk = new BulkingAgent<UserStatus>(2, 3000)

bulk.BulkProduced |> Observable.guiSubscribe (fun data ->
  web.Output.StartList "Message bulk (%d)" data.Length
  for d in data do 
    web.Output.AddItem "%s" d.Status )

twitter.TweetReceived 
|> Observable.add bulk.Enqueue

twitter.StartSearch [ "mercury" ]
twitter.Cancel()

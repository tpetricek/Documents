#r "references/FSharp.Data.dll"
#r "bin/Debug/Twitter.API.dll"
#load "references/GuiExtensions.fs"

open System
open System.Threading
open System.Windows.Forms
open System.Collections.Generic

open FSharp.Control
open FSharp.WebBrowser
open FSharp.TwitterAPI

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
let twitter = connector.Connect("8134140")

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


// Display live search data 
web.Output.StartList()

let search = Twitter.searchTweets twitter ["catchphrase"]
search.TweetReceived |> Observable.guiSubscribe (fun status ->
    match status.Text, status.User with
    | Some text, Some user ->
        web.Output.AddItem "<strong>%s</strong>: %s" user.Name text
    | _ -> ()  )
search.Start()
search.Stop()

Twitter.requestRawData twitter "https://api.twitter.com/1.1/search/tweets.json" ["q", "a"]

open System.Net
open System.IO

do 
  let url = "https://api.twitter.com/1.1/friends/ids.json?user_id=123"
  let req = WebRequest.Create(url)
  req.Method
  req.AddOAuthHeader(twitter, ["user_id", "123"])
  
  let s = req.Headers.["Authorization"]
  let oa =
    [ for s in s.Substring(6).Split(',') do
        let [| k; v |] = s.Split('=')
        yield k, v.Substring(1, v.Length - 2) ]
  let qa = [for k,v in oa -> k + "=" + v] |> String.concat "&"
  
  let req = WebRequest.Create(url + "&" + qa)
  req.Headers.Add(HttpRequestHeader.Authorization, s)


  let resp = req.GetResponse()
  let strm = resp.GetResponseStream()
  let sr = new StreamReader(strm)
  sr.ReadToEnd() 
  |> ignore

do
  let url = "https://api.twitter.com/1.1/search/tweets.json"
  let queryString = "q=fsharp" //[for k, v in query -> k + "=" + v] |> String.concat "&"
  let req = WebRequest.Create(url + "?" + queryString, Method="GET")
  req.AddOAuthHeader(twitter, ["q", "fsharp"])

  let resp = req.GetResponse()
  let strm = resp.GetResponseStream()
  let sr = new StreamReader(strm)
  sr.ReadToEnd() 
  |> ignore

do              
  let req = WebRequest.Create("https://api.twitter.com/1.1/statuses/home_timeline.json") 
  req.AddOAuthHeader(twitter, [])
  let resp = req.GetResponse()
  let strm = resp.GetResponseStream()
  let sr = new StreamReader(strm)
  sr.ReadToEnd() 
  |> ignore

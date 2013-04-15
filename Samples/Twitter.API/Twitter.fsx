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
let connector = Twitter.Authenticate(key, secret, web.Navigate)

// NOTE: Run all code up to this point. A window should appear. You can then
// login to twitter and you'll get a pin code that you need to copy and
// paste as an argument to the 'Connect' method below:
let twitter = connector.Connect("3253143")

// Login: 'fsharpd'
// Password: 'fsharp123'

// ----------------------------------------------------------------------------
// Using Twitter APIs
// ----------------------------------------------------------------------------

// Download the current home timeline
web.Output.StartList()

let home = Twitter.Timelines.HomeTimeline(twitter)
for item in home do
  web.Output.AddItem "<strong>%s</strong>: %s" item.User.Name item.Text
 

// Display stream with live data
web.Output.StartList()

let sample = Twitter.Streaming.SampleTweets(twitter)
sample.TweetReceived |> Observable.guiSubscribe (fun status ->
    match status.Text, status.User with
    | Some text, Some user ->
        web.Output.AddItem "<strong>%s</strong>: %s" user.Name text
    | _ -> ()  )
sample.Start()
sample.Stop()


// Display live search data 
web.Output.StartList()

let search = Twitter.Streaming.FilterTweets(twitter, ["#childhoodfears"])
search.TweetReceived |> Observable.guiSubscribe (fun status ->
    match status.Text, status.User with
    | Some text, Some user ->
        web.Output.AddItem "<strong>%s</strong>: %s" user.Name text
    | _ -> ()  )
search.Start()
search.Stop()

// Get list of friend IDs
let friends = Twitter.Followers.FriendsIds(twitter)
friends.Ids 

// Get details about firends (up to 100)
let friendInfos = Twitter.Users.Lookup(twitter, friends.Ids |> Seq.take 100)
for friend in friendInfos do
  printfn "%s (@%s)" friend.Name friend.ScreenName





(*
For testing purposes, call other things...


Twitter.RequestRawData(twitter, "https://api.twitter.com/1.1/search/tweets.json", ["q", "fsharp"])
Twitter.RequestRawData(twitter, "https://api.twitter.com/1.1/followers/ids.json")
Twitter.RequestRawData(twitter, "https://api.twitter.com/1.1/users/lookup.json", ["user_id", "880772426,464211199"])
|> (fun s -> System.IO.File.WriteAllText(System.IO.Path.Combine(__SOURCE_DIRECTORY__, "references/users_lookup.json"), s))
*)
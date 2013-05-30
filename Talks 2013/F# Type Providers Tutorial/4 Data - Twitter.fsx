#r "lib/FSharp.Data.dll"
#r "lib/Twitter.API.dll"
#load "lib/GuiExtensions.fs"

open System
open System.Threading
open System.Windows.Forms
open System.Collections.Generic

open FSharp.Data
open FSharp.Control
open FSharp.WebBrowser
open FSharp.TwitterAPI

// ----------------------------------------------------------------------------
// Create user interface and connect to Twitter
// ----------------------------------------------------------------------------

let web = WebBrowser.Create()
let key = "CoqmPIJ553Tuwe2eQgfKA"
let secret = "dhaad3d7DreAFBPawEIbzesS1F232FnDsuWWwRTUg"
let connector = Twitter.Authenticate(key, secret, web.Navigate)

// NOTE: Run all code up to this point. A window should appear. You can then
// login to twitter and you'll get a pin code that you need to copy and
// paste as an argument to the 'Connect' method below:
let twitter = connector.Connect("7089667")

// Login: 'fsharpd'
// Password: 'fsharp123'

// ----------------------------------------------------------------------------
// Using Twitter APIs (Using nice wrappers created with JSON type provider)
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

let search = Twitter.Streaming.FilterTweets(twitter, ["boston"])
search.TweetReceived |> Observable.guiSubscribe (fun status ->
    match status.Text, status.User with
    | Some text, Some user ->
        web.Output.AddItem "<strong>%s</strong>: %s" user.Name text
    | _ -> ()  )
search.Start()
search.Stop()

// Get a list of friends (followed people) and followers
let friends = Twitter.Followers.FriendsIds(twitter)
let followers = Twitter.Followers.FollowerIds(twitter)
friends.Ids |> Seq.length
followers.Ids |> Seq.length

// Get details about firends (up to 100)
let friendInfos = Twitter.Users.Lookup(twitter, friends.Ids |> Seq.take 100)
for friend in friendInfos do
  printfn "%s (@%s)\t\t%d" friend.Name friend.ScreenName friend.Id

// Get friends list for a specified user (@dsyme)
let friends2 = Twitter.Followers.FriendsIds(twitter, userId=25663453L)
friends2.Ids |> Seq.length

// Get information about connections between @dsyme and @tomaspetricek
let fs = Twitter.Friendships.Show(twitter, 25663453L, 18388966L)
fs.Relationship.Source.ScreenName
fs.Relationship.Target.ScreenName

// Search recent tweets with the #fsharp tag
let fsharp = Twitter.Search.Tweets(twitter, "fsharp", count=100)
for status in fsharp.Statuses do
  printfn "@%s: %s" status.User.ScreenName status.Text

// ----------------------------------------------------------------------------
// Requesting RAW data - Use JSON provider to wrap these nicely!
// ----------------------------------------------------------------------------

//
// Suggested users
//  * https://dev.twitter.com/docs/api/1.1/get/users/suggestions
//
Twitter.RequestRawData(twitter, "https://api.twitter.com/1.1/users/suggestions.json")

//
// TODO:
//
//  * Get a sample response using the above script
//  * Use a type provider to generate type:
//      type MyType = JsonProvider<"filename.json">
//  * Use the generated type to parse & read the data
//      MyType.Parse can parse the downloaded result string
//

// 
// Trends for a given place
//  * https://dev.twitter.com/docs/api/1.1/get/trends/place
//
Twitter.RequestRawData(twitter, "https://api.twitter.com/1.1/trends/place.json", ["id", "1"])
#load "AgentUtils.fs"

open System
open System.Net
open System.Threading
open System.Collections.Generic
open FSharp.Control

let root = "http://www.guardian.co.uk"

// ----------------------------------------------------------------------------
// Completely simple proxy 

let proxy1 = HttpServer.Start("http://localhost:8082/", fun ctx -> async {
  let url = root + ctx.Request.Url.PathAndQuery
  let wc = new WebClient()
  let! html = wc.AsyncDownloadString(Uri(url))
  let html = html.Replace(root, "http://localhost:8082")
  let html = html.Replace("<title>", "<title>Proxy: ")
  do! ctx.Response.AsyncReply(html) })

proxy1.Stop()

// ----------------------------------------------------------------------------
// Forwarding data in chunks asynchronously

let proxy2 = HttpServer.Start("http://localhost:8082/", fun ctx -> async {
  let url = root + ctx.Request.Url.PathAndQuery
  ctx.Response.SendChunked <- true

  // Initialize HTTP connection to the server
  let request = HttpWebRequest.Create(url)
  use! response = request.AsyncGetResponse()
  use stream = response.GetResponseStream()

  // Asynchronous loop to copy data
  let count = ref 1
  let buffer = Array.zeroCreate 4096
  while !count > 0 do
    let! read = stream.AsyncRead(buffer, 0, buffer.Length)
    do! ctx.Response.OutputStream.AsyncWrite(buffer, 0, read)    
    count := read
  ctx.Response.Close() })

proxy2.Stop()

// ----------------------------------------------------------------------------
// Implementing cache using agents

type CacheMessage =
  | TryGet of string * AsyncReplyChannel<option<string>>
  | Add of string * string

let cache = Agent.Start(fun agent -> async {
  // Store cached pages in a mutable dictionary
  let pages = new Dictionary<_, _>()
  while true do
    let! msg = agent.Receive()
    match msg with 
    | TryGet(url, repl) ->
        // Try to return page from the cache
        match pages.TryGetValue(url) with
        | true, html -> repl.Reply(Some(html))
        | _ -> repl.Reply(None)
    | Add(url, html) ->
        // Add downloaded page to the cache
        let html = html.Replace("<title>", "<title>Cache: ")
        pages.[url] <- html })

// ----------------------------------------------------------------------------
// Simple web proxy with cache

let proxy3 = HttpServer.Start("http://localhost:8082/", fun ctx -> async {
  let url = root + ctx.Request.Url.PathAndQuery
  let! cached = cache.PostAndAsyncReply(fun ch -> TryGet(url, ch))
  match cached with 
  | Some(html) -> 
      // Return page from the cache
      ctx.Response.Reply(html)
  | _ ->
      // Download page and add it to the cache
      let wc = new WebClient()
      let! html = wc.AsyncDownloadString(Uri(url))
      let html = html.Replace(root, "http://localhost:8082")
      cache.Post(Add(url, html))
      ctx.Response.Reply(html) })

proxy3.Stop()



#r "HtmlAgilityPack.dll"
open System
open System.Net
open System.Collections.Concurrent
open System.Text.RegularExpressions
open System.Threading
open HtmlAgilityPack

// ----------------------------------------------------------------------------
// Helper functions for extracting stuff from HTML
// ----------------------------------------------------------------------------

/// Extract all links from the document that start with "http://"
let extractLinks (doc:HtmlDocument) = 
  try
    [ for a in doc.DocumentNode.SelectNodes("//a") do
        if a.Attributes.Contains("href") then
          let href = a.Attributes.["href"].Value
          if href.StartsWith("http://") then 
            let endl = href.IndexOf('?')
            yield if endl > 0 then href.Substring(0, endl) else href ]
  with _ -> []

/// Extract the <title> of the web page
let getTitle (doc:HtmlDocument) =
  try
    let title = doc.DocumentNode.SelectSingleNode("//title")
    if title <> null then title.InnerText.Trim() else "Untitled"
  with _ -> "Untitled"

// ----------------------------------------------------------------------------
// Asynchronous crawler
// ----------------------------------------------------------------------------

/// Asynchronously download the document and parse the HTML
let downloadDocument url = async {
  try
    use wc = new WebClient()
    let! html = wc.AsyncDownloadString(Uri(url))
    let doc = HtmlDocument()
    doc.LoadHtml(html)
    return doc 
  with e ->
    return HtmlDocument() }

// This version of the sample uses asynchronous blocking queue
// implemented as an F# agent. This replaces .NET BlockingCollection
// with a similar collection that does not block while taking or
// adding elements.

#r "AsyncAgents\\bin\\Debug\\AsyncAgents.dll"
open AsyncAgents

let pending = new BlockingQueueAgent<string>()
let visited = new ConcurrentDictionary<string, bool>()

let rec crawler() = async {
  let! url = pending.AsyncTake()
  if not (visited.ContainsKey(url)) then
    // Download 'url' and add to visited
    visited.TryAdd(url, true) |> ignore
    let! doc = downloadDocument(url)
    if doc <> null then
      for link in extractLinks(doc) do
        do! pending.AsyncAdd(link)
      printfn "[%s]\n%s\n" url (getTitle doc)
  return! crawler() }


// Start crawling
[ for i in 0 .. 100 -> crawler() ]
|> Async.Parallel 
|> Async.Ignore
|> Async.Start

pending.AsyncAdd("http://www.guardian.co.uk/")
|> Async.Start

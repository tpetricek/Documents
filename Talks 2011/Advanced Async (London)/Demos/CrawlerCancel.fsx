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


let pending = new BlockingCollection<string>()
let visited = new ConcurrentDictionary<string, bool>()

let rec crawler() = async {
  let url = pending.Take()
  if not (visited.ContainsKey(url)) then
    // Download 'url' and add to visited
    visited.TryAdd(url, true) |> ignore
    let! doc = downloadDocument(url)
    if doc <> null then
      for link in extractLinks(doc) do
        pending.Add(link)
      printfn "[%s]\n%s\n" url (getTitle doc)
  return! crawler() }


// Start crawling
let work = 
  [ for i in 0 .. 1 -> crawler() ]
  |> Async.Parallel 
  |> Async.Ignore

// F# asynchronous workflows pass CancellationToken around
// implicitly, so there is no need for adding parameter and
// inserting explicit checks. We just specify the token when
// calling 'Async.Start'

let source = new CancellationTokenSource()
Async.Start(work, source.Token)

pending.Add("http://www.guardian.co.uk/")
source.Cancel()
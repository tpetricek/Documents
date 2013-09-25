#r @"..\Providers\Xml.TypeProvider\bin\debug\Xml.TypeProvider.dll"
#r "System.Xml.Linq.dll"
open XmlProvider

// Print data from XML file using inferred structure
let test = new StructuredXml<"Philosophy.xml">()
for a in test.Root.GetAuthorElements() do
  printfn "%s %s (%A)" a.Name a.Surname a.Birth
  for b in a.GetBookElements() do
    printfn " - %s" b.Title

// Print data from RSS feed using inferred structure
let feed = new StructuredXml<"http://www.fssnip.net/Pages/rss">()
let ch = feed.Root.GetChannelElements() |> Seq.head
for itm in ch.GetItemElements() do
  printfn "%s" (itm.GetTitleElements() |> Seq.head).Element.Value

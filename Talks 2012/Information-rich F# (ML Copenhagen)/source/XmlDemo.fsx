// ---------------------------------------------------------
// Load type provider for XML and .NET XML library
// ---------------------------------------------------------

#r "lib/Xml.TypeProvider.dll"
#r "System.Xml.Linq.dll"

// ---------------------------------------------------------
// Process news feed
// ---------------------------------------------------------

// Download RSS feed from the Guardian web site
[<Literal>]
let guardian = "http://feeds.guardian.co.uk/theguardian/world/rss"
 
type XmlRss = XmlProvider.StructuredXml<guardian>
let feed = XmlRss()

// Read the titles & see what is happening today!
















// ---------------------------------------------------------
// Process local file that is changing
// ---------------------------------------------------------

type XmlBooks = XmlProvider.StructuredXml<"test.xml">
let test = new XmlBooks()

for a in test.Root.GetAuthorElements() do
  printfn "%s %s (%d)" a.Name a.Surname a.Birth
  for b in a.GetBookElements() do
    printfn " - %s" b.Title





















#nowarn "40"
namespace FSharpWeb.Core

// ----------------------------------------------------------------------------

/// Simple record that models the target structure
/// (A listing with name & link and list of articles with 
/// a title, description and an URL)
type Listing = 
  { Name : string
    Link : string
    Items : seq<string * string * string> }


// ----------------------------------------------------------------------------
// Transformations 

module Model =
  open System.Text.RegularExpressions

  /// Helper function that strips HTML from a 
  /// string and takes first 200 characters
  let stripHtml (html:string) = 
    let res = Regex.Replace(html, "<.*?>", "")
    if res.Length > 200 then res.Substring(0, 200) + "..." else res

  // --------------------------------------------------------------------------
  // Example #1 - Parsing RSS feeds

  // Specifies the structure of the source XML document
  // A union case name corresponds to elemnets in XML document, so for example
  //   type Title = Title of string
  //
  // will match a XML node like:
  //   <title>hello world</title>
  //
  type Title = Title of string
  type Link = Link of string
  type Description = Description of string

  type Item = Item of Title * Link * Description
  type Channel = Channel of Title * Link * Description * list<Item>
  type Rss = Rss of Channel
  
  /// Load latest news from the Guardian RSS feed
  let loadGuardian() =
    // Load data and specify that names in XML are all lowercase
    let url = "http://feeds.guardian.co.uk/theguardian/world/rss"
    let doc = StructuralXml.Load(url, LowerCase = true)

    // Match the data against a type modeling the RSS feed structure
    match doc.Root with
    | Rss(Channel(Title title, Link link, _, items)) -> 
        // And convert the data to the F# Listing type
        let items = seq { 
          for (Item(Title title, Link link, Description descr)) in items do
            yield title, link, stripHtml descr }
        { Name = title; Link = link; Items = items }

  // --------------------------------------------------------------------------
  // Example #2 - Parsing a (well-formed) XHTML document

  // Specifies the structure of the source XML document
  // Multiple union cases mean that there may be multiple different nested 
  // elements and a list is used if they can be repeated. Note that it is
  // also possible to create recursive references (e.g. for nested <div>):
  // 
  //   type HtmlElement = 
  //     | H1 of string
  //     | Div of HtmlElements
  //   and HtmlElements = 
  //     list<HtmlElement>
  //
  // will match a XML node like:
  //
  //   <div>
  //     <h1>Hello</h1>
  //     <div><div><h1>world</h1></div></div>
  //   </div>

  type HtmlElement = 
    | Div of HtmlElements
    | P of HtmlElements
    | H1 of string
    | H2 of string

  and HtmlElements = 
    list<HtmlElement>

  type Body = Body of HtmlElements
  type Html = Html of Body


  /// Simple processing function that finds all 
  /// headings in the specified XML structure
  let rec findHeading = Seq.collect (function
    | Div els 
    | P els -> findHeading els
    | H1 heading 
    | H2 heading -> seq [ heading ])
  
  /// Load data from a well-formed XHTML web site
  let loadTomasP() =
    let url = "http://tomasp.net/blog"
    let ns = "http://www.w3.org/1999/xhtml"
    let doc = StructuralXml.Load(url, LowerCase = true, Namespace = ns)

    // Match the data against the structure & extract Body element
    let (Html(Body elements)) = doc.Root 
    // Build 'Listing' record with just headings from the page
    { Name = "TomasP.Net"
      Link = "http://tomasp.net"
      Items = seq { for h in findHeading elements -> h.Trim(), "", "" } }
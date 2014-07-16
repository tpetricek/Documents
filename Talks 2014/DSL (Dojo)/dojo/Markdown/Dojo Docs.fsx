open System
open System.Windows.Forms

// ------------------------------------------------------------------
// In this Dojo, we look at processing simple documents. The 
// idea is that we can read Markdown documents into a simple
// discriminated union structure and then apply processing 
// functions to them...
// ------------------------------------------------------------------

// The following part of code just opens a WinForms window simple 
// window with an HTML preview for showing the formatted results.

let wb = new WebBrowser(Dock=DockStyle.Fill)
let frm = new Form(Width=800, Height=600, Visible=true)
frm.Controls.Add(wb)
let showHtml html = 
  wb.DocumentText <- """<html><style>body {padding:30px;font:150% cabin}
    li {margin-top:10px}</style><body>""" + html + "</body></html>"

// Domain model - this is the key part where we define what a document
// is. In our sample, it is quite simple and supports just a few cases:

type MarkdownNode = 
  | Literal of string              // Plain text 
  | Strong of MarkdownNode         // Bold node
  | Paragraph of MarkdownNode      // Paragraph node
  | Heading of int * MarkdownNode  // Heading with specified size
  | List of MarkdownNode list      // <ul> list with number of elements
  | Sequence of MarkdownNode list  // Sequencing of number of nodes

// Now, we're going to load a parser for the Markdown format that
// turns a string into MarkdownNode. The implementation is in 
// 'Parser.fs', but it is not the point of this dojo. You can find
// detailed description of this in a free chapter from F# Deep Dives
// (http://manning.com/petricek2)

let doc = Literal ""

let sample = """
# Creating DSLs with F#

Key components of a DSL:

 * **Model** describes the 
   structure of the domain 
   that we are modelling

 * **Syntax** provides an easy 
   way for solving problems 
   using the DSL
"""

let oslo = """
# Buildings and structures

Architecture in Oslo may at first seem dull. Unlike for 
instance its Swedish counterpart, Stockholm, downtown Oslo 
has only scattered monumental buildings where in particular 
the Parliament-Palace axis (upper part of Karl Johan Street) 
has a certain Parisian grandeur. The charm of Oslo can also 
be found in the affluent inner-city suburbs of for instance 
Frogner and Fagerborg as well as above St.Hanshaugen park. 

 * **Royal Palace** - Tours inside the palace are arranged 
   in summertime, this year from June 21. The tickets for the 
   tour must be bought in advance from a post office. 

 * **University in Oslo** - The building is currently only 
   housing the Faculty of Law, the rest of the university is 
   situated at Blindern. Occasional concerts will be arranged in 
   the magnificent Universitetets Aula, housing 11 of Edvard 
   Munch's pictures. 

 * **Opera House** - Norway's first entry into the top league 
   of modern architecture. Awarded the 2008 prize for best 
   cultural building at the World Architecture Festival in Barcelona, 
   and the prestigious Mies van der Rohe award for best European 
   contemporary architecture in 2009, its appearance is stunning. 
"""

// F# Interactive behaves in a bit silly way here and so it would
// tell you that it cannot load the file even though it actually can..
// So, just select the gray code inside the following block and run it!
#if DEMO

#load "Parser.fs"
let doc = Parser.parseMarkdown sample
let doc = Parser.parseMarkdown oslo

#endif

// ------------------------------------------------------------------
// Now comes the fun part! We need to write recursive functions
// to process the document and do something interesting with it.
// ------------------------------------------------------------------

// The first function turns a document into HTML. For
// Literal, we just return the string. For other nodes, we
// recursively process the nested nodes and concatentate
// the resulting strings..

let rec formatNode doc = 
  match doc with
  | Literal s -> s
  | Strong s -> sprintf "<strong>%s</strong> " (formatNode s)
  | Paragraph p -> formatNode p
  | Heading(n, p) -> sprintf "<h%d>%s</h%d>" n (formatNode p) n
  | List items -> 
      let lis = [ for li in items -> sprintf "<li>%s</li>" (formatNode li) ]
      "<ul>" + (String.concat "" lis) + "</ul>"
  | Sequence nodes -> nodes |> List.map formatNode |> String.concat ""

// Take document, format it and show it!
doc
|> formatNode
|> showHtml

// Now, let's look at some exercises! The first task is to count
// the number of words in a document. To do that, you need to write
// a function similar to 'formatNode' that takes a document and 
// uses pattern matching to handle each of the cases. When you 
// get a 'Literal' node, you should count the number of words in 
// a string. For all other nodes, use recursion and sum the words
// in child nodes.

let rec countWords doc = 
  match doc with
  | Literal s -> 
      // TODO: Count words in 's'
      0
  | Strong node -> countWords node

  // TODO: Add all other cases until the compiler is
  // happy and does not give a warning on 'match doc with'


countWords doc


// The next exercise is to write a transformation that takes a 
// document and returns a new document. For example, you can turn
// all text in the document to UPPERCASE (and make it angry :-))
// To do that, you need to follow similar pattern as above, but
// this time, you'll return a new document node:

let rec uppercase doc = 
  match doc with 
  | Literal s ->
      // TODO: Return a new 'Literal' node containing the
      // upper-case version of the text in 's'.
      failwith "TODO"

  | Strong node ->
      // Recursively process the nested node and 
      // re-wrap it in the 'Strong' element.
      let upperNode = uppercase node
      Strong upperNode

  // TODO: All all the other cases to make the compiler happy...


// Show uppercase version of the document!
doc
|> uppercase
|> formatNode
|> showHtml
open System
open System.Windows.Forms

// -------------------------------------------------------
// Show HTML
// -------------------------------------------------------

let wb = new WebBrowser(Dock=DockStyle.Fill)
let frm = new Form(Width=800, Height=600, Visible=true, TopMost=true)
frm.Controls.Add(wb)
let showHtml html = 
  wb.DocumentText <- """<html><style>body {padding:30px;font:150% cabin}
    li {margin-top:10px}</style><body>""" + html + "</body></html>"

// -------------------------------------------------------
// Domain model
// -------------------------------------------------------

// TODO: MarkdownNode is:
//  - Literal 
//  - Strong/Paragraph/Heading
//  - List
//  - Sequence

// -------------------------------------------------------
// Formatting
// -------------------------------------------------------

// DEMO: A sample document

// TODO: Format literal, strong, paragraph (formatNode)
// DEMO: Format the rest of Markdown

// TODO: Make the DSL nicer (! and $)

// -------------------------------------------------------
// Translating documents
// -------------------------------------------------------

#r "../packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
open FSharp.Data

// TODO: http://mymemory.translated.net/api/get?q=hello&langpair=en%7Cno
// TODO: Create a simple translate function
// DEMO: Add a few more tweaks

// TODO: Translate literal, strong, heading (translateNode)
// DEMO: Handle the rest of the nodes

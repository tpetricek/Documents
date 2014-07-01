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
// Formatting
// -------------------------------------------------------

// TODO: Heading 1 "Creating DSLs with F#"
// TODO: Paragraph "Key components of a DSL:"
// TODO: Strong
// DEMO: List function
// DEMO: List sample

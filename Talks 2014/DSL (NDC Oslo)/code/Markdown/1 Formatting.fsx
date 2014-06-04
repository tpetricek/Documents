open System
open System.Windows.Forms

// -------------------------------------------------------
// Show HTML
// -------------------------------------------------------

let wb = new WebBrowser(Dock=DockStyle.Fill)
let frm = new Form(Width=800, Height=600, Visible=true)
frm.Controls.Add(wb)
let showHtml html = 
  wb.DocumentText <- "<html><body style='font:150% calibri'>" + html + "</body></html>"

// -------------------------------------------------------
// Formatting
// -------------------------------------------------------

let heading n text = 
  sprintf "<h%d>%s</h%d>" n text n
let strong text = 
  sprintf "<strong>%s</strong>" text
let p text = 
  sprintf "<p>%s</p>" text
let list items =
  let lis = [ for i in items -> sprintf "<li>%s</li>" i ]
  "<ul>" + (String.concat "" lis) + "</ul>"

(heading 1 "Creating DSLs with F#") + 
(p "Key components of a DSL:") + 
(list [
  (strong "Model") + " describes the structure of the domain that we are modelling";
  (strong "Syntax") + " provides an easy way for solving problems using the DSL" ])
|> showHtml

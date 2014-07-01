open System
open System.Windows.Forms

// -------------------------------------------------------
// Show HTML
// -------------------------------------------------------

let wb = new WebBrowser(Dock=DockStyle.Fill)
let frm = new Form(Width=800, Height=600, Visible=true)
frm.Controls.Add(wb)
let showHtml html = 
  wb.DocumentText <- """<html><style>body {padding:30px;font:150% cabin}
    li {margin-top:10px}</style><body>""" + html + "</body></html>"

// -------------------------------------------------------
// Domain model
// -------------------------------------------------------

type MarkdownNode = 
  | Literal of string
  | Strong of MarkdownNode 
  | Paragraph of MarkdownNode 
  | Heading of int * MarkdownNode 
  | List of MarkdownNode list
  | Sequence of MarkdownNode list

// -------------------------------------------------------
// Formatting
// -------------------------------------------------------

let rec formatNode = function
  | Literal s -> s
  | Strong s -> sprintf "<strong>%s</strong> " (formatNode s)
  | Paragraph p -> formatNode p
  | Heading(n, p) -> sprintf "<h%d>%s</h%d>" n (formatNode p) n
  | List items -> 
      let lis = [ for li in items -> sprintf "<li>%s</li>" (formatNode li) ]
      "<ul>" + (String.concat "" lis) + "</ul>"
  | Sequence nodes -> nodes |> List.map formatNode |> String.concat ""

// -------------------------------------------------------
// Sample document
// -------------------------------------------------------

let doc =
  Sequence 
    [ Heading(1, Literal "Creating DSLs with F#")
      Paragraph(Literal "Key components of a DSL:")
      List(
        [ Sequence
            [ Strong (Literal "Model")
              Literal " describes the structure of the domain that we are modelling"]
          Sequence 
            [ Strong (Literal "Syntax")
              Literal " provides an easy way for solving problems using the DSL" ]
      ]) ]

doc
|> formatNode
|> showHtml

// -------------------------------------------------------
// Building nicer syntax
// -------------------------------------------------------

let (!) s = Literal s
let ($) s1 s2 = Sequence [s1;s2]

let doc2 =
  Heading(1, !"Creating DSLs with F#") $
  Paragraph(!"Key components of a DSL:") $
  List(
    [ Strong (!"Model") $
        Literal " describes the structure of the domain that we are modelling"
      Strong (!"Syntax") $
        Literal " provides an easy way for solving problems using the DSL" ])

doc2
|> formatNode
|> showHtml

// -------------------------------------------------------
// Translating documents
// -------------------------------------------------------

#r "../packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
open FSharp.Data

[<Literal>]
let Sample = "http://mymemory.translated.net/api/get?q=hello&langpair=en%7Cno"

// Request sample translation
Http.Request(Sample).Body

// Generate types for translation API
type Translate = JsonProvider<Sample>

let translate (phrase:string) = 
  printfn "Translating: '%s'" phrase
  if String.IsNullOrWhiteSpace(phrase) then "" else
  let phrase = phrase.Replace("F#", "fsharp")
  let doc = Translate.Load("http://mymemory.translated.net/api/get?langpair=en|no&de=tomas@tomasp.net&q=" + phrase)
  let phrase = doc.Matches.[0].Translation
  phrase.Replace("fsharp", "F#")

translate "world"
translate "describes the structure of the domain that we are modelling"

// Recursive function to translate documents
let rec translateNode = function
  | Literal text -> Literal (translate text)
  | Strong s -> Strong(translateNode s)
  | Heading(n, s) -> Heading(n, translateNode s)
  | List li -> List (List.map translateNode li)
  | Paragraph p -> Paragraph(translateNode p)
  | Sequence p -> Sequence (List.map translateNode p)

doc
|> translateNode
|> formatNode
|> showHtml

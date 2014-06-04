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
// Translating documents
// -------------------------------------------------------

#r "../packages/FSharp.Data.2.0.8/lib/net40/FSharp.Data.dll"
open FSharp.Data

[<Literal>]
let Sample = "http://mymemory.translated.net/api/get?q=hello&langpair=en%7Cno"

// Generate types for translation API
type Translate = JsonProvider<Sample>

let translate (phrase:string) = 
  printfn "Translating: '%s'" phrase
  if String.IsNullOrWhiteSpace(phrase) then "" else
  let phrase = phrase.Replace("F#", "fsharp")
  let doc = Translate.Load("http://mymemory.translated.net/api/get?langpair=en|no&de=tomas@tomasp.net&q=" + phrase)
  let phrase = doc.Matches.[0].Translation
  phrase.Replace("fsharp", "F#")

// Recursive function to translate documents
let rec translateNode = function
  | Literal text -> Literal (translate text)
  | Strong s -> Strong(translateNode s)
  | Heading(n, s) -> Heading(n, translateNode s)
  | List li -> List (List.map translateNode li)
  | Paragraph p -> Paragraph(translateNode p)
  | Sequence p -> Sequence (List.map translateNode p)

// -------------------------------------------------------
// Active patterns for parsing
// -------------------------------------------------------

let toString chars =
  System.String(chars |> Array.ofList)

let (|StartsWith|_|) prefix list =
  let rec loop = function
    | [], rest -> Some(rest)
    | p::prefix, r::rest when p = r -> loop (prefix, rest)
    | _ -> None
  loop (prefix, list)

let rec parseBracketedBody closing acc = function
  | StartsWith closing (rest) -> Some(List.rev acc, rest)
  | c::chars -> parseBracketedBody closing (c::acc) chars
  | _ -> None
  
let (|Delimited|_|) delim = function
  | StartsWith delim chars -> parseBracketedBody delim [] chars
  | _ -> None

// -------------------------------------------------------
// Parsing spans
// -------------------------------------------------------
  
let rec parseSpans acc chars = seq {
  let emitLiteral() = seq {
    if acc <> [] then 
      yield acc |> List.rev |> toString |> Literal }
  match chars with
  | Delimited ['*'; '*' ] (body, chars) ->
      yield! emitLiteral ()
      yield Strong(Sequence(parseSpans [] body |> List.ofSeq))
      yield! parseSpans [] chars
  | c::chars ->
      yield! parseSpans (c::acc) chars
  | [] ->
      yield! emitLiteral () }

// -------------------------------------------------------
// Helper functions & patterns
// -------------------------------------------------------

module List = 
  let partitionWhile f = 
    let rec loop acc = function
      | x::xs when f x -> loop (x::acc) xs
      | xs -> List.rev acc, xs
    loop [] 

let (|LineSeparated|) lines =
  let isWhite = System.String.IsNullOrWhiteSpace
  match lines |> List.partitionWhile (isWhite >> not) with
  | par, _::rest | par, ([] as rest) -> par, rest
    
let (|AsCharList|) (str:string) = 
  str |> List.ofSeq

let (|PrefixedLines|) prefix (lines:list<string>) = 
  let prefixed, other = lines |> List.partitionWhile (fun line -> line.StartsWith(prefix))
  [ for line in prefixed -> line.Substring(prefix.Length) ], other

// -------------------------------------------------------
// Parsing Markdown blocks
// -------------------------------------------------------

let rec parseBlocks lines = seq {
  match lines with
  | AsCharList(StartsWith ['#'; ' '] heading)::rest ->
      yield Heading(1, Sequence(parseSpans [] heading |> List.ofSeq))
      yield! parseBlocks rest
  | LineSeparated (h::tl, rest) when h.StartsWith(" *") -> 
      let body = String.concat " " (h.Substring(2)::tl) |> List.ofSeq
      let list, rest = 
        parseBlocks rest |> List.ofSeq
        |> List.partitionWhile (function List _ -> true | _ -> false)
      yield List [
        yield Sequence(parseSpans [] body |> List.ofSeq)
        for List l in list do yield! l ]
      yield! rest 
  | LineSeparated (body, rest) when body <> [] -> 
      let body = String.concat " " body |> List.ofSeq
      yield Paragraph(Sequence(parseSpans [] body |> List.ofSeq))
      yield! parseBlocks rest 
  | line::rest when System.String.IsNullOrWhiteSpace(line) ->
      yield! parseBlocks rest 
  | _ -> () }

let parseMarkdown (sample:string) = 
  Sequence(parseBlocks (sample.Split('\r', '\n') |> List.ofSeq) |> List.ofSeq)

// -------------------------------------------------------
// Samples
// -------------------------------------------------------

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

sample
|> parseMarkdown
|> translateNode
|> formatNode
|> showHtml


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


let res = 
  oslo
  |> parseMarkdown
  |> translateNode

res
|> formatNode
|> showHtml


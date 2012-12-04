#r "System.Web.dll"
#r "FSharp.Markdown.dll"
#r "FSharp.CodeFormat.dll"
#load "StringParsing.fs"

open System
open System.IO
open System.Web
open System.Collections.Generic

open FSharp.Patterns
open FSharp.CodeFormat
open FSharp.Markdown

// ----------------------------------------------------------------------------

let dir = __SOURCE_DIRECTORY__ 
let template = File.ReadAllText(dir + "\\template.html")

let fsharpCompiler = @"C:\Program Files (x86)\Microsoft F#\v4.0\FSharp.Compiler.dll"
let asm = System.Reflection.Assembly.LoadFile(fsharpCompiler)
let formatAgent = CodeFormat.CreateAgent(asm)

// ------------------------------------------------------------------------------------------------

/// Extract source code from all CodeBlock elements in the document
let rec collectCodes par = seq {
  match par with 
  | CodeBlock(code) ->
      yield code
  | Matching.ParagraphNested(_, nested) ->
      for par in nested |> Seq.concat do
        yield! collectCodes par
  | _ -> () }

/// Repalce CodeBlock elements with formatted 
/// HTML that was processed by the F# snippets tool 
let rec replaceParCodes (codeLookup:IDictionary<_, _>) = function
  | CodeBlock(code) ->
      let html : string = codeLookup.[code]
      Some(HtmlBlock(html))
  | Matching.ParagraphNested(pn, nested) ->
      Matching.ParagraphNested(pn, List.map (List.choose (replaceParCodes codeLookup)) nested) |> Some
  | Matching.ParagraphSpans(ps, spans) -> Matching.ParagraphSpans(ps, spans) |> Some
  | Matching.ParagraphLeaf(pl) -> Matching.ParagraphLeaf(pl) |> Some

// ------------------------------------------------------------------------------------------------

// Main function - process all files in the specified directory
// (and keep a relative path to the root for inserting links)
let build file =
  let source = dir + "\\" + file
  let target = source.Replace(".text", ".html")
  let text = File.ReadAllText(source)
  let doc = Markdown.Parse(text)
  let paragraphs = doc.Paragraphs

  // Extract all CodeBlocks and pass them to F# snippets
  let codes = paragraphs |> Seq.collect collectCodes |> Array.ofSeq
  let pars, tipHtml = 
    if codes.Length = 0 then paragraphs, ""
    else
      // Process all F# snippets using a tool
      let fsharpSource = 
        codes |> Seq.mapi (fun index code ->
            "// [snippet:" + (string index) + "]\n" +
            code + "\n" +
            "// [/snippet]" )
        |> String.concat "\n\n"

      // Write to Temp
      File.WriteAllText(target + ".fs", fsharpSource)
      let snippets, errors = formatAgent.ParseSource("C:\\temp\\Blog.fsx", fsharpSource, "")
      let formatted = CodeFormat.FormatHtml(snippets, "ft", false, false)
      let snippetLookup = Array.zip codes [| for fs in formatted.SnippetsHtml -> fs.Html |] |> dict
        
      // Replace CodeBlocks with formatted code    
      List.choose (replaceParCodes snippetLookup) paragraphs, formatted.ToolTipHtml

  // Construct new Markdown document and write it
  let newDoc = MarkdownDocument(pars, doc.DefinedLinks)
  let proc = Markdown.WriteHtml(newDoc)
  File.WriteAllText(target, String.Format(template, proc, tipHtml))

// ----------------------------------------------------------------------------

build "test.text"

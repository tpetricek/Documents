#r "nunit.framework.dll"
open System
open NUnit.Framework

// ----------------------------------------------------------------------------

type MarkdownSpan =
  | Literal of string
  | InlineCode of string
  | Strong of MarkdownSpans
  | Emphasis of MarkdownSpans
  | HyperLink of MarkdownSpans * string
  | HardLineBreak

and MarkdownSpans = list<MarkdownSpan>

type MarkdownBlock = 
  | Heading of int * MarkdownSpans
  | Paragraph of MarkdownSpans
  | CodeBlock of list<string>

type MarkdownDocument = list<MarkdownBlock>

// ----------------------------------------------------------------------------

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
  
let (|Bracketed|_|) opening closing = function
  | StartsWith opening chars -> parseBracketedBody closing [] chars
  | _ -> None
let (|Delimited|_|) delim = (|Bracketed|_|) delim delim
  
let rec parseSpans acc chars = seq {
  let emitLiteral() = seq {
    if acc <> [] then 
      yield acc |> List.rev |> toString |> Literal }

  match chars with
  | StartsWith [' '; ' '; '\n'; '\r'] chars
  | StartsWith [' '; ' '; '\n' ] chars
  | StartsWith [' '; ' '; '\r' ] chars -> 
      yield! emitLiteral ()
      yield HardLineBreak
      yield! parseSpans [] chars
  | Delimited ['`'] (body, chars) ->
      yield! emitLiteral ()
      yield InlineCode(toString body)
      yield! parseSpans [] chars
  | Delimited ['*'; '*' ] (body, chars)
  | Delimited ['_'; '_' ] (body, chars) ->
      yield! emitLiteral ()
      yield Strong(parseSpans [] body |> List.ofSeq)
      yield! parseSpans [] chars
  | Delimited ['*' ] (body, chars)
  | Delimited ['_' ] (body, chars) ->
      yield! emitLiteral ()
      yield Emphasis(parseSpans [] body |> List.ofSeq)
      yield! parseSpans [] chars
  | Bracketed ['['] [']'] (body, Bracketed ['('] [')'] (url, chars)) ->
      yield! emitLiteral ()
      yield HyperLink(parseSpans [] body |> List.ofSeq, toString url)
      yield! parseSpans [] chars
  | c::chars ->
      yield! parseSpans (c::acc) chars
  | [] ->
      yield! emitLiteral () }

// ----------------------------------------------------------------------------

module List = 
  let partitionWhile f = 
    let rec loop acc = function
      | x::xs when f x -> loop (x::acc) xs
      | xs -> List.rev acc, xs
    loop [] 

// ----------------------------------------------------------------------------

let (|LineSeparated|) lines =
  let isWhite = System.String.IsNullOrWhiteSpace
  match lines |> List.partitionWhile (isWhite >> not) with
  | par, _::rest | par, ([] as rest) -> par, rest
    
let (|AsCharList|) (str:string) = 
  str |> List.ofSeq

let (|PrefixedLines|) prefix (lines:list<string>) = 
  let prefixed, other = lines |> List.partitionWhile (fun line -> line.StartsWith(prefix))
  [ for line in prefixed -> line.Substring(prefix.Length) ], other

let (PrefixedLines "..." res) = ["...1"; "...2"; "3" ]

let rec parseBlocks lines = seq {
  match lines with
  | AsCharList(StartsWith ['#'; ' '] heading)::rest ->
      yield Heading(1, parseSpans [] heading |> List.ofSeq)
      yield! parseBlocks rest
  | AsCharList(StartsWith ['#'; '#'; ' '] heading)::rest ->
      yield Heading(2, parseSpans [] heading |> List.ofSeq)
      yield! parseBlocks rest
  | PrefixedLines "    " (body, rest) when body <> [] ->
      yield CodeBlock(body)
      yield! parseBlocks rest
  | LineSeparated (body, rest) when body <> [] -> 
      let body = String.concat " " body |> List.ofSeq
      yield Paragraph(parseSpans [] body |> List.ofSeq)
      yield! parseBlocks rest 
  | line::rest when System.String.IsNullOrWhiteSpace(line) ->
      yield! parseBlocks rest 
  | _ -> () }

// ----------------------------------------------------------------------------

let sample = @"
  # Visual F#
  
  F# is a **programming language** that supports _functional_, as       
  well as _object-oriented_ and _imperative_ programming styles.        

  ## Introduction
  Hello world can be written as follows:                                

      printfn ""Hello world!""

  ## References
  For more information, see the [F# home page] (http://fsharp.net) or 
  read [Real-World Func tional Programming](http://manning.com/petricek) 
  published by [Manning](http://manning.com)."

let doc = parseBlocks (sample.Split('\r', '\n') |> List.ofSeq) |> List.ofSeq

// ----------------------------------------------------------------------------

// TODO: Implement formatting of Markdown documents as HTML

open System.IO

let rec formatSpan (output:TextWriter) = function
  | _ -> failwith "Not implemented" 

let rec formatBlock (output:TextWriter) = function
  | _ -> failwith "Not implemented" 

// Test
let sb = System.Text.StringBuilder()
let output = new StringWriter(sb)
doc |> Seq.iter (formatBlock output)
sb.ToString()

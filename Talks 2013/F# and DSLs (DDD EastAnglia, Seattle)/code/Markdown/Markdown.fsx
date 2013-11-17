open System

// -------------------------------------------------------
// Markdown domain model
// -------------------------------------------------------

type MarkdownSpan =
  | Literal of string
  | InlineCode of string
  | Strong of MarkdownSpans
  | Emphasis of MarkdownSpans
  | HyperLink of MarkdownSpans * string
  | HardLineBreak

and MarkdownSpans = list<MarkdownSpan>

// -------------------------------------------------------
// Active patterns for parsing
// -------------------------------------------------------

/// A parameterized active pattern that checks
/// if 'list' starts with a given 'prefix' (and returns
/// the rest of the input)
let (|StartsWith|_|) prefix list =
  let rec loop = function
    | [], rest -> Some(rest)
    | p::prefix, r::rest when p = r -> loop (prefix, rest)
    | _ -> None
  loop (prefix, list)

/// Parse input that matches the scheme: <opening> .* <closing> .*
let (|Bracketed|_|) opening closing input = 
  let rec parseBracketedBody closing acc = function
    | StartsWith closing (rest) -> Some(List.rev acc, rest)
    | c::chars -> parseBracketedBody closing (c::acc) chars
    | _ -> None
  match input with
  | StartsWith opening chars -> parseBracketedBody closing [] chars
  | _ -> None

// Detect string delimited with some characters: <delim> .* <delim> .*
let (|Delimited|_|) delim = (|Bracketed|_|) delim delim 

// -------------------------------------------------------
// Writing Markdown parser
// -------------------------------------------------------

let toString chars =
  System.String(chars |> Array.ofList)

let rec (|Command|_|) = function 
  | StartsWith [' '; ' '; '\n'; '\r'] chars
  | StartsWith [' '; ' '; '\n' ] chars
  | StartsWith [' '; ' '; '\r' ] chars -> 
      Some(chars, HardLineBreak) 
  | Delimited ['`'] (body, chars) ->
      Some(chars, InlineCode(toString body))
  | Delimited ['*'; '*' ] (body, chars)
  | Delimited ['_'; '_' ] (body, chars) ->
      Some(chars, Strong(parseSpans [] body |> List.ofSeq))
  | Delimited ['*' ] (body, chars)
  | Delimited ['_' ] (body, chars) ->
      Some(chars, Emphasis(parseSpans [] body |> List.ofSeq))
  | Bracketed ['['] [']'] (body, Bracketed ['('] [')'] (url, chars)) ->
      Some(chars, HyperLink(parseSpans [] body |> List.ofSeq, toString url))
  | _ -> None

// -------------------------------------------------------
// Putting things together
// -------------------------------------------------------

and parseSpans acc chars = seq {
  let emitLiteral() = seq {
    if acc <> [] then 
      yield acc |> List.rev |> toString |> Literal }
  match chars with
  | Command(chars, cmd) ->
        yield! emitLiteral ()
        yield cmd
        yield! parseSpans [] chars
  | c::chars ->
      yield! parseSpans (c::acc) chars
  | [] ->
      yield! emitLiteral () }

// ----------------------------------------------------------------------------

let sample = 
  "The _most important_ F# keyword is `let`. For more " +
  "see [F# web site](http://www.fsharp.net)."

sample
|> List.ofSeq
|> parseSpans []
|> List.ofSeq

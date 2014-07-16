module Parser

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

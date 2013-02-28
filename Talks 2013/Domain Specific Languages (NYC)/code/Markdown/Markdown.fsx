#r "nunit.framework.dll"
open System
open NUnit.Framework

// ----------------------------------------------------------------------------

// Markdown inline formatting
type MarkdownSpan =
  | Literal of string
  | InlineCode of string
  | Strong of MarkdownSpans
  | Emphasis of MarkdownSpans
  | HyperLink of MarkdownSpans * string
  | HardLineBreak

and MarkdownSpans = list<MarkdownSpan>

// ----------------------------------------------------------------------------

let toString chars =
  System.String(chars |> Array.ofList)

// ----------------------------------------------------------------------------

// DEMO: A parameterized active pattern that checks
// if 'list' starts with a given 'prefix' (and returns
// the rest of the input)
let (|StartsWith|_|) prefix list =
  let rec loop = function
    | [], rest -> Some(rest)
    | p::prefix, r::rest when p = r -> loop (prefix, rest)
    | _ -> None
  loop (prefix, list)

// DEMO: Parse input until we reach 'closing' sub-list
let rec parseBracketedBody closing acc = function
  | StartsWith closing (rest) -> Some(List.rev acc, rest)
  | c::chars -> parseBracketedBody closing (c::acc) chars
  | _ -> None

// DEMO: Parse input that matches the scheme
//   <opening> .* <closing> .*
let (|Bracketed|_|) opening closing = function
  | StartsWith opening chars -> parseBracketedBody closing [] chars
  | _ -> None

// TODO: Implement an active pattern (using 'Bracketed')
// that tests whether input contains a sequence:
//   <delim> .* <delim> .*
let (|Delimited|_|) delim = (|Bracketed|_|) delim delim

// ----------------------------------------------------------------------------
    
let rec parseSpans acc chars = seq {
  // emit literal if we skipped some characters
  let emitLiteral() = seq {
    if acc <> [] then 
      yield acc |> List.rev |> toString |> Literal }

  // now we can use nice pattern matching to detect spans!
  match chars with
  | Delimited ['`'] (body, chars) ->
      yield! emitLiteral ()
      yield InlineCode(toString body)
      yield! parseSpans [] chars

  // TODO: Add lots of other features using active patterns!

  | c::chars ->
      yield! parseSpans (c::acc) chars
  | [] ->
      yield! emitLiteral () }

// ----------------------------------------------------------------------------



let sample = 
  "The _most important_ F# keyword is `let`. For more " +
  "see [F# web site](http://www.fsharp.net)."

parseSpans [] (List.ofSeq sample)



// ----------------------------------------------------------------------------

[<TestFixture>]
module Tests = 

  [<Test>]
  let ``Recognize line breaks`` () = 
    let res = "a  \n" |> List.ofSeq |> parseSpans [] |> List.ofSeq
    Assert.That((res = [Literal "a"; HardLineBreak]))

  [<Test>]
  let ``Recognize strong text`` () = 
    let res = "a**a**" |> List.ofSeq |> parseSpans [] |> List.ofSeq
    Assert.That((res = [Literal "a"; Strong [Literal "a"]]))

  [<Test>]
  let ``Recognize emphasis using underscore`` () = 
    let res = "a _b_ c" |> List.ofSeq |> parseSpans [] |> List.ofSeq
    Assert.That((res = [Literal "a "; Emphasis [Literal "b"]; Literal " c"]))

  [<Test>]
  let ``Recognize emhpasis using star`` () =
    let res = "a _b_ c" |> List.ofSeq |> parseSpans [] |> List.ofSeq
    Assert.That((res = [Literal "a "; Emphasis [Literal "b"]; Literal " c"]))

  [<Test>]
  let ``Recognize hyperlinks`` () = 
    let res = "a [text](http://url) c" |> List.ofSeq |> parseSpans [] |> List.ofSeq
    Assert.That((res = [Literal "a "; HyperLink ([Literal "text"],"http://url"); Literal " c"]))

// ----------------------------------------------------------------------------

  do
    ``Recognize strong text`` () 
    ``Recognize emphasis using underscore`` () 
    ``Recognize emhpasis using star`` ()
    ``Recognize hyperlinks`` () 

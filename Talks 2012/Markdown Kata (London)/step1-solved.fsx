#r "nunit.framework.dll"
open System
open NUnit.Framework

// ----------------------------------------------------------------------------

let toString chars =
  System.String(chars |> Array.ofList)

let rec parseInlineBody acc chars = 
  match chars with 
  | '`'::rest 
  | ([] as rest) -> List.rev acc, rest
  | c::chars -> parseInlineBody (c::acc) chars

let parseInline = function
  |'`'::chars -> Some(parseInlineBody [] chars)
  | _ -> None

// ----------------------------------------------------------------------------

open NUnit.Framework

[<TestFixture>]
module Tests = 

  // parseInlineBody

  [<Test>]
  let ``End of inline is found`` () =
    let res = "aa`bb" |> List.ofSeq |> parseInlineBody []
    Assert.That((res = (['a'; 'a'], ['b'; 'b'])))

  [<Test>]
  let ``All input is consumed`` () =
    let res = "aa" |> List.ofSeq |> parseInlineBody []
    Assert.That((res = (['a'; 'a'], [])))

  // parseInline
  [<Test>]
  let ``Needs backtick`` () =
    let res = "aa" |> List.ofSeq |> parseInline
    Assert.That((res = None))

  [<Test>]
  let ``Finds inline code`` () =
    let res = "`aa`bb" |> List.ofSeq |> parseInline
    Assert.That((res = Some (['a'; 'a'], ['b'; 'b'])))
  
  do
    ``End of inline is found``()
    ``All input is consumed``()
    ``Needs backtick`` ()
    ``Finds inline code`` ()

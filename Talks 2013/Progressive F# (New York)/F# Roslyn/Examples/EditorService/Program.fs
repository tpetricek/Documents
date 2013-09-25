open System
open Microsoft.FSharp.Compiler
open Microsoft.FSharp.Compiler.SourceCodeServices

// ----------------------------------------------------------------------------
// F# editor services
// ----------------------------------------------------------------------------

// Create an interactive checker instance (ignore notifications)
let checker = InteractiveChecker.Create(NotifyFileTypeCheckStateIsDirty ignore)

let parseWithTypeInfo (file, input) = 
  // We first need to get the untyped info
  let checkOptions = checker.GetCheckOptionsFromScriptRoot(file, input, DateTime.Now, [| |])
  let untypedRes = checker.UntypedParse(file, input, checkOptions)
  // This might need some time - wait until all DLLs are loaded etc.
  let rec waitForTypeCheck(n) = async {
    let typedRes = checker.TypeCheckSource(untypedRes, file, 0, input, checkOptions, IsResultObsolete(fun _ -> false), null)
    match typedRes with
    | TypeCheckAnswer.TypeCheckSucceeded(res) -> return untypedRes, res
    | res when n > 100 -> return failwithf "Parsing did not finish... (%A)" res
    | _ -> 
        do! Async.Sleep(100)
        return! waitForTypeCheck(n + 1) }
  waitForTypeCheck 0 |> Async.RunSynchronously

// ----------------------------------------------------------------------------

let input = 
  """
  let foo() = 
    let msg = "Hello world"
    if true then 
      printfn "%s" msg.
  """
let inputLines = input.Split('\n')
let file = "/home/user/Test.fsx"

// Get untyped & typed information and get code for the IDENT token (needed later)
let identToken = Parser.tagOfToken(Parser.token.IDENT("")) 
let untyped, parsed = parseWithTypeInfo(file, input)

// Get tool tip at the specified location
let tip = parsed.GetDataTipText((1, 7), inputLines.[1], ["foo"], identToken)
printfn "%A" tip

// Get declarations (autocomplete) for a location
let decls = 
  parsed.GetDeclarations(Some untyped, (4, 23), inputLines.[4], ([], "msg"), fun _ -> false)
  |> Async.RunSynchronously
for item in decls.Items do
  printfn " - %s" item.Name

// TASK #3: Given a sample FSX file as an input, write something (command line
// application will do for now), that reads names of variables from the input
// finds all occurrences of the variables in the source file & prints their
// type information.

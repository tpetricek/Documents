open System
open System.IO
open Microsoft.FSharp.Compiler.ErrorLogger
open Microsoft.FSharp.Compiler.Interactive.Shell

// ----------------------------------------------------------------------------
// Embedding F# interactive
// ----------------------------------------------------------------------------

let sbOut = new Text.StringBuilder()
let sbErr = new Text.StringBuilder()
let argv = System.Environment.GetCommandLineArgs()

do
  // Initialize F# Interactive evaluation session
  // (We ignore all generated output & error stream)
  let inStream = new StringReader("")
  let outStream = new StringWriter(sbOut)
  let errStream = new StringWriter(sbErr)
  let fsi = FsiEvaluationSession([|"C:\\test.exe"; "--noninteractive"|], inStream, outStream, errStream)

  while true do
    try      
      let text = Console.ReadLine()
      if text.StartsWith("=") then 
        // Evaluate an expression and print the result
        match fsi.EvalExpression(text.Substring(1)) with
        | Some value -> printfn "%A" value.ReflectionValue
        | None -> printfn "Got no result!"
      else
        // Evaluate top-level interaction which is not an expression
        // (such as 'let foo = 10')
        fsi.EvalInteraction(text)
        printfn "Ok"

    with e ->
      match e.InnerException with
      | null -> printfn "Error evaluating expression (%s)" e.Message
      | WrappedError(err, _) -> printfn "Error evaluating expression (%s)" err.Message
      | _ -> printfn "Error evaluating expression (%s)" e.Message
      

// TASK #4: Embed F# interactive in Quake 3!
// https://twitter.com/TIHan/status/371723680801234944
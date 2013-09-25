open Microsoft.FSharp.Compiler.SourceCodeServices
open Microsoft.FSharp.Data.TypeProviders

// ----------------------------------------------------------------------------
// DEMO #1: Calling the F# compiler tokenizer
// ----------------------------------------------------------------------------

// Initialize tokenizer and give it some file name (does not have to exist)
let sourceTok = SourceTokenizer([], "C:\\test.fsx")

let tokenizeLines (lines:string[]) =
  [ // State of the tokenizer at the end of the line
    let state = ref 0L
    for n, line in lines |> Seq.zip [ 0 .. lines.Length ] do
      // Create tokenizer for a single line
      let tokenizer = sourceTok.CreateLineTokenizer(line)
      let rec parseLine() = seq {
        // Continue scanning & reading tokens until the end
        match tokenizer.ScanToken(!state) with
        | Some(tok), nstate ->
            // Extract the original text from the line..
            let str = line.Substring(tok.LeftColumn, tok.RightColumn - tok.LeftColumn + 1)
            yield str, tok
            state := nstate
            yield! parseLine()
        | None, nstate -> state := nstate }
      yield n, parseLine() |> List.ofSeq ]

// ----------------------------------------------------------------------------
// Tokenize sample F# input

let tokenizedLines = 
  tokenizeLines
    [| "// Sets the hello wrold variable"
       "let hello = \"Hello world\" " |]

// Print information abotu tokens
for lineNo, lineToks in tokenizedLines do
  printfn "%d:  " lineNo
  for str, info in lineToks do 
    // 'info' contains various information about the token
    printfn "       [%s:'%s']" info.TokenName str


// TASK #1: Write spell checker for F# code!
// You can iterate over all tokens, get text in comments and
// spell check all words in comments. (possibly also check identifiers)

// Calling dictionary web service using type providers
type Dict = WsdlService<"http://services.aonaware.com/DictService/DictService.asmx">
let svc = Dict.GetDictServiceSoap()

// Look for exact match for the 'world' word
let res = svc.Match("world", "exact")
for word in res do
  printfn "%A" word.Word
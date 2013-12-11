open System
open System.Collections

for v in Seq.cast<DictionaryEntry> (Environment.GetEnvironmentVariables()) do
  let name = v.Key :?> string
  let value = v.Value :?> string
  printfn "%s - %s" name value




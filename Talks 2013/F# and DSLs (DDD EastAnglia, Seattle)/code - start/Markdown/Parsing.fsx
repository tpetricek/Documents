// Getting serious: Parsing formulas 

let tryParseFormula (s:string) =
  if s.StartsWith("=") then Some(s.Substring(1).Trim()) 
  else None

let tryParseNumber (s:string) = 
  match Int32.TryParse(s) with
  | true, num -> Some num
  | _ -> None

let parser s = 
  printfn "nop"

parser "= 4 + 4"
parser "4"

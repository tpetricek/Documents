#load "lib/Parsers.fs"
open System.IO
open Parsers

// --------------------------------------------------------
// Working with sequences
// --------------------------------------------------------

let rec searchFiles dir = seq {
  yield! Directory.GetFiles(dir) 
  for subdir in Directory.GetDirectories(dir) do
    yield! searchFiles subdir }

searchFiles @"C:\tomas\research\thesis\text"
|> Seq.iter (printfn "%s")

// --------------------------------------------------------
// Writing parsers
// --------------------------------------------------------

let rec oneOrMore p = parse { 
  let! x = p
  let! xs = zeroOrMore p
  return x::xs }

and zeroOrMore p = parse {
  return! oneOrMore p
  return [] }

// Examples: Parse one or more & zero or more numbers
run (oneOrMore number) "-10"
run (oneOrMore number) "123 + 45"
run (zeroOrMore number) "-10"
run (zeroOrMore number) "123 + 45"


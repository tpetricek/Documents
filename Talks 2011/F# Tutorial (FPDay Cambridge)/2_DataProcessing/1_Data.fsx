module StockData

open System
open System.IO

// ------------------------------------------------------------------
// Introduction

// Printing different types of things
//   %s - string
//   %f - floating point number
//   %A - any .NET object
// printfn "%A %f - %s" DateTime.Now 99.9 "Message"

// Read all data from a file into an array
let lines = File.ReadAllLines(__SOURCE_DIRECTORY__ + "\\data\\aapl.csv")

// Print all the lines 
let test () = 

  for line in lines do 
    printfn "%s" line

// ------------------------------------------------------------------
// Parsing CSV data

// Get first & second line from the input
let line1 = lines |> Seq.head
let line2 = lines |> Seq.skip 1 |> Seq.head

// Split string using .NET method
let str = "Hello world"
let arr = str.Split([| ' ' |])
arr.[0]

// Convert strings to numbers or dates
float "12.34"
DateTime.Parse("1985-05-02")

// ==================================================================
// TASK #1
// Iterate over lines in the file using 'for' (don't forget to skip
// the first line which contains headings), parse the date and 
// Open, High, Low, Close values and print the date together with 
// average value: (O+H+L+C) / 4.0

// (...)

// ------------------------------------------------------------------
// Basic sequence expressions

let nums = [ 1 .. 10 ]

// Sequences are evaluated on demand
let oddSeq = 
  seq { for n in nums do
          if n%2=1 then yield n }

// Arrays are evaluated immediately 
let oddArr = 
  [| for n in nums do
          if n%2=1 then yield n |]

// ==================================================================
// TASK #2
// Take the previous for loop that parses CSV file and turn it
// into a sequence expression (creating array or list) that contains
// pairs of DateTime and tuple with four float values (OHLC)

// An example of a value that we want to return
let value = DateTime.Now, (10.0, 20.0, 9.0, 19.0)

// (...)

// ==================================================================
// TASK #3
// Encapsulate the functionality into a function that takes the
// name of the stock (assuming file exists), parses the CSV
// and returns data as an array

let loadStockPrices name =
  failwith "not implemented!"
  [| value |]


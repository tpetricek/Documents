#r "System.Xml.Linq.dll"
#load @"packages\FsLab.0.0.6-beta\FsLab.fsx"
open FSharp.Net
open FSharp.Data

// ------------------------------------------------------------------
// WORD #1
//
// Use the WorldBank type provider to get all countries in the 
// "North America" region, then find country "c" with the smallest
// "Life expectancy at birth, total (years)" value in the year 2000
// and return the first two letters of the "c.Code" property
// ------------------------------------------------------------------

let wb = WorldBankData.GetDataContext()
wb.Countries.``Czech Republic``.Indicators.``Population (Total)``.[2000]


// ------------------------------------------------------------------
// WORD #2
//
// Read the RSS feed in "data/bbc.xml" using XmlProvider and return
// the last word of the title of an article that was published at
// 9:05am (the date does not matter, just hours & minutes)
// ------------------------------------------------------------------

type Sample = XmlProvider<"data/Writers.xml">
let doc = Sample.GetSample()

// ------------------------------------------------------------------
// WORD #3
//
// Get the ID of a director whose name contains "Haugerud" and then
// search for all movie credits where he appears. Then return the 
// second (last) word from the movie he directed (the resulting type
// has properties "Credits" and "Crew" - you need movie from the 
// Crew list (there is only one).
// ------------------------------------------------------------------

// Make HTTP request to /3/search/person
let key = "6ce0ef5b176501f8c07c634dfa933cff"
let name = "craig"
let data = 
  Http.Request
    ( "http://api.themoviedb.org/3/search/person",
      query = [ ("query", "craig"); ("api_key", key) ],
      headers = ["accept", "application/json"] )

// Parse result using JSON provider
type PersonSearch = JsonProvider<"data/personsearch.json">
let sample = PersonSearch.Parse(data)

let first = sample.Results |> Seq.head
first.Name

// Request URL: "http://api.themoviedb.org/3/person/<id>/movie_credits
// (You can remove the 'query' parameter because it is not needed here)

// Use JsonProvider with sample file "data/moviecredits.json" to parse

// ------------------------------------------------------------------
// WORD #4
//
// Use CsvProvider to parse the file "data/LibraryCalls.csv" which
// contains information about some PHP library calls (got it from the
// internet :-)). Note that the file uses ; as the separator.
//
// Then find row where 'params' is equal to 2 and 'count' is equal to 1
// and the 'name' column is longer than 6 characters. Return first such
// row and get the last word of the function name (which is separated
// by underscores). Make the word plural by adding 's' to the end.
// ------------------------------------------------------------------

type Lib = CsvProvider<"data/MortalityNY.tsv", Separator="\t">
let lib = new Lib()

// ------------------------------------------------------------------
// WORD #5
//
// Use Freebase type provider to find chemical element with 
// "Atomic number" equal to 36 and then return the 3rd and 2nd 
// letter from the *end* of its name (that is 5th and 6th letter
// from the start).
// ------------------------------------------------------------------

let fb = FreebaseData.GetDataContext()

// You'll need "Science and Technology" and then "Chemistry"
query { for e in fb.``Science and Technology``.Astronomy.Stars do 
        where e.Distance.HasValue
        select (e.Name, e.Distance) } 
|> Seq.toList

// ------------------------------------------------------------------
// WORD #6
//
// Use CsvProvider to load the Titanic data set from "data/Titanic.csv"
// (using the default column separator) and find the name of a female
// passenger who was 19 years old and embarked in the prot marked "Q"
// Then return Substring(19, 3) from her name.
// ------------------------------------------------------------------

// Use "data/titanic.csv" as the sample file


// ------------------------------------------------------------------
// WORD #7
//
// Using the same RSS feed as in Word #2 ("data/bbc.xml"), find
// item that contains "Duran Duran" in the title and return the
// 14th word from its description (split the description using ' '
// as the separator and get item at index 13).
// ------------------------------------------------------------------

// (...)
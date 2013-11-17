#r "../packages/FSharp.Data.1.1.10/lib/net40/FSharp.Data.dll"
#r "../packages/FSharp.Data.Experimental.1.1.10/lib/net40/FSharp.Data.Experimental.dll"
open FSharp.Data
open FSharp.Net

// ------------------------------------------------------------------
// Movie database using JSON

let key = "api_key=6ce0ef5b176501f8c07c634dfa933cff"
let data = 
  Http.Request
    ( "http://api.themoviedb.org/3/search/movie?query=star+wars&" + key,
      headers = ["accept", "application/json"] )

type Search = JsonProvider<"movie.json">
let movies = Search.Parse(data)

// Print all movies in the query result
for movie in movies.Results do
  printfn "%A" movie.Title

// ------------------------------------------------------------------
// Movie database

let db = new ApiaryProvider<"themoviedb">("http://api.themoviedb.org")
db.AddQueryParam("api_key", "6ce0ef5b176501f8c07c634dfa933cff")

let res = db.Search.Person(query=["query","craig"])
for person in res.Results do
  printfn "%d %s" person.Id person.Name

let person = db.Person.GetPerson("8784")
person.PlaceOfBirth

for cast in person.CombinedCredits().Cast do
  if cast.Title.IsSome then
    printfn "%s (%s)" cast.Title.Value.String.Value cast.Character

// ------------------------------------------------------------------
// Accessing other APIs

let fs = new ApiaryProvider<"fssnip">("http://api.fssnip.net/")

let snips = fs.Snippet.List()
for snip in snips do 
  printfn "%s" snip.Title

let snip = fs.Snippet.GetSnippet("fj")
snip.Tags
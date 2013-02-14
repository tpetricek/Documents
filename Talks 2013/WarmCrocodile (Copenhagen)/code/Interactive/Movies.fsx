#r "../packages/FSharp.Data.1.0.13/lib/net40/FSharp.Data.dll"
#r "../packages/FSharp.Data.Experimental.1.0.2/lib/net40/FSharp.Data.Experimental.dll"
open FSharp.Data

// ------------------------------------------------------------------
// Movie database

let db = new ApiaryProvider<"themoviedb">("http://api.themoviedb.org")
db.AddQueryParam("api_key", "6ce0ef5b176501f8c07c634dfa933cff")

let res = db.Search.Person(query=["query","craig"])
for person in res.Results do
  printfn "%d %s" person.Id person.Name

let person = db.Person.GetPerson("8784")
person.PlaceOfBirth

for cast in person.Credits().Cast do
  printfn "%s (%s)" cast.Title cast.Character

// ------------------------------------------------------------------
// Accessing other APIs

let fs = new ApiaryProvider<"fssnip">("http://api.fssnip.net/")

let snips = fs.Snippet.List()
for snip in snips do 
  printfn "%s" snip.Title

let snip = fs.Snippet.GetSnippet("fj")
snip.Tags

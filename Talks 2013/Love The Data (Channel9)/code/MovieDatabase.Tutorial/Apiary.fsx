// Movie database
#r @"..\packages\FSharp.Data.Experimental.1.1.1\lib\net40\FSharp.Data.Experimental.dll"
open FSharp.Data

let db = new ApiaryProvider<"themoviedb">("http://api.themoviedb.org")
db.AddQueryParam("api_key", "6ce0ef5b176501f8c07c634dfa933cff")

let res = db.Search.Person(query=["query","craig"])
for person in res.Results do
  printfn "%d %s" person.Id person.Name

let person = db.Person.GetPerson("8784")

for cast in person.Credits().Cast do
  printfn "%s (%s)" cast.Title.String.Value cast.Character


// Accessing other APIs
let fs = new ApiaryProvider<"fssnip">("http://api.fssnip.net/")

let snips = fs.Snippet.List()
for snip in snips do 
  printfn "%s" snip.Title
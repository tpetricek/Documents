#r @"..\packages\FSharp.Data.1.1.1\lib\net40\FSharp.Data.dll"
open FSharp.Net
open FSharp.Data

// Movie database using JSON
type MovieSearch = JsonProvider<"moviesearch.json">

let key = "api_key=6ce0ef5b176501f8c07c634dfa933cff"
let data = 
  Http.Request
    ( "http://api.themoviedb.org/3/search/movie?query=star+wars&" + key,
      headers = ["accept", "application/json"] )

let movies = MovieSearch.Parse(data)
for res in movies.Results do
  printfn "%s" res.Title
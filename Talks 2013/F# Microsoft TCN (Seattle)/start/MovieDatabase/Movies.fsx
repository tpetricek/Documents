#r "../packages/FSharp.Data.1.1.10/lib/net40/FSharp.Data.dll"
#r "../packages/FSharp.Data.Experimental.1.1.10/lib/net40/FSharp.Data.Experimental.dll"
open FSharp.Data
open FSharp.Net

// Movie database using JSON
let key = "api_key=6ce0ef5b176501f8c07c634dfa933cff"
let data = 
  Http.Request
    ( "http://api.themoviedb.org/3/search/movie?query=star+wars&" + key,
      headers = ["accept", "application/json"] )



// http://docs.themoviedb.apiary.io/
//
// "http://api.themoviedb.org"
// "api_key" -> "6ce0ef5b176501f8c07c634dfa933cff"
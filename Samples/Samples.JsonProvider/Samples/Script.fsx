#r "System.Xml.Linq.dll"
#r "../References/FunJS.dll"
#load "../common/json.fs"
#load "../common/network.fs"
#load "../TypeProviders/jsoninference.fs"
#load "../ApiaryProvider/apiary.fs"

open System
open System.IO
open System.Net
open System.Reflection

open FSharp.Web
open FSharp.Net
open FSharp.Web.JsonUtils
open FSharp.Apiary
open FSharp.Apiary.SchemaReader

// JsonParser.parse """   { "a": "http:\/\/t.co\/oUxXm7cb" } """
// JsonParser.parse """   { "text":"\u3010\u5b9a\u671f\u3011\u30a2\u30e1\u30d6\u30ed\u3084\u3063\u3066\u307e\u3059\u3002\u826f\u304b\u3063\u305f\u3089http:\/\/t.co\/8mSwdH5k" } """

// ----------------------------------------------------------------------------------------------

let rec printNames spaces = function
  | Entity(name, ops, nested) ->
      printfn "%s%s" spaces ("get_" + name)
      for op in ops do
        printfn "%s     (%A)" spaces op
      nested |> Seq.iter (printNames (spaces + "   "))
  | Module(name, nested) ->
      printfn "%s%s" spaces name
      nested |> Seq.iter (printNames (spaces + "   "))
  | Function(name, v) ->
      printfn "%s- %s (%A)" spaces name v

let names = getOperationTree "themoviedb"
asRestApi names |> List.iter (printNames "") 

// ----------------------------------------------------------------------------------------------


JsonParser.parse (wget "http://api.apiary.io/blueprint/resources/themoviedb")


let meth, patho = downloadOperations "themoviedb" |> Seq.nth 36


let name = "themoviedb"
let path = Uri.EscapeDataString(patho)
let uri = sprintf "http://api.apiary.io/blueprint/snippet/%s/%s/%s" name meth path
let spec = JsonParser.parse (wget uri)

spec?description.Text
spec?incoming?body

[ for h, v in spec?incoming?headers.Properties -> h, v.Text ]


spec?outgoing

Http.wgetEx
  "https://api.twitter.com/1.1/statuses/user_timeline.json "
  ["Authorization", """OAuth oauth_consumer_key="CoqmPIJ553Tuwe2eQgfKA", oauth_nonce="8b3715d53ba42b8455c62b1f65feaaee", oauth_signature="ijk%2Bm6oSq%2FJNDmnuzNiixfJaYmk%3D", oauth_signature_method="HMAC-SHA1", oauth_timestamp="1351560604", oauth_token="18388966-zguHHrNrBdZTmoiAU8gWJkHLAemgAMlGgYcALt4PO", oauth_version="1.0" """ ]
  []

Http.wgetEx 
  "http://private-91d9-themoviedb.apiary.io/3/search/movie"
  [("Accept", "application/json"); ("test", "123")]
  [("query", "batman"); ("api_key", "6ce0ef5b176501f8c07c634dfa933cff")]

let samples = 
  [ for example in spec?outgoing do
      if example?status.Text = "200" then 
        let source = example?body.Text
        yield JsonParser.parse source ]

JsonInference.inferTypeFromCollection samples



wget "http://api.apiary.io/blueprint/snippet/themoviedb/GET/%2F3%2Faccount"
wget "https://api.apiary.io/blueprint/snippet/themoviedb/GET/%2F3%2Faccount"

let json = """[  {    "id": "2",    "name": "Pavel",    "surname": "Novak",    "shortname": "Streamer",    "city": null,    "country": "0",    "state": null,    "created": null,    "timezone": "",    "email": "novak@email.cz",    "invoiceEmail": null,    "company": "My company, s.r.o.",    "street": null,    "street2": null,    "type": "user",    "zip": null  } ]"""
JsonParser.parse json

let wb = JsonParser.parse (File.ReadAllText(__SOURCE_DIRECTORY__ + "/worldbank.json"))
JsonInference.inferType wb

fsi.PrintDepth <- 6


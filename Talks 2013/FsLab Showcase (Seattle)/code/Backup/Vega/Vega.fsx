module Vega
#load "HttpServer.fs"
#load "Grammar.fs"
#load "Basics.fs"

open System.IO
open System.Diagnostics

let private server = ref None

let show browse vega = 
  File.WriteAllText(__SOURCE_DIRECTORY__ + "\\spec.json", vega)
  match !server with 
  | None -> server := Some (FSharp.Http.HttpServer.Start("http://localhost:8081/", __SOURCE_DIRECTORY__))
  | _ -> ()
  if browse then Process.Start("http://localhost:8081") |> ignore

let stop () = match !server with Some s -> s.Stop() | _ -> ()
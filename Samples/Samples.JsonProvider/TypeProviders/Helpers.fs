namespace FSharp.ProviderImplementation

// ----------------------------------------------------------------------------------------------
// Helpers for writing type providers
// ----------------------------------------------------------------------------------------------

open System
open System.IO
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices

module Helpers =
  let openStreamInProvider (cfg:TypeProviderConfig) (fileName:string) = 
    // Resolve the full path or full HTTP address
    let resolvedFileOrUri = 
      if not (fileName.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)) then
        Path.Combine(cfg.ResolutionFolder, fileName)
      else fileName

    // Open network stream or file stream
    if resolvedFileOrUri.StartsWith("http") then
      let req = System.Net.WebRequest.Create(Uri(resolvedFileOrUri))
      let resp = req.GetResponse() 
      resp.GetResponseStream()
    else
      File.OpenRead(resolvedFileOrUri) :> Stream

  let readFileInProvider cfg fileName = 
    use stream = openStreamInProvider cfg fileName
    use reader = new StreamReader(stream)
    reader.ReadToEnd()

[<AutoOpen>]
module AutoHelpers =
  // Helper active pattern to avoid warnings in InvokeCode etc.
  let (|Singleton|) = function [l] -> l | _ -> failwith "Parameter mismatch"


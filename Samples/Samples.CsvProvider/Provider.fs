// Copyright (c) Microsoft Corporation 2005-2011.
// This sample code is provided "as is" without warranty of any kind. 
// We disclaim all warranties, either express or implied, including the 
// warranties of merchantability and fitness for a particular purpose. 

namespace Samples.FSharp.CsvProvider

open System
open System.Reflection
open System.IO
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open System.Text.RegularExpressions

type FieldType = 
    | Float
    | DateTime
    | String

type CsvRow(data:string[]) = 
    let inv = System.Globalization.CultureInfo.InvariantCulture

    member x.GetString(i) = data.[i]
    member x.GetDateTime(i) = DateTime.Parse(data.[i], inv)
    member x.GetDouble(i) = Double.Parse(data.[i], inv)

// Simple type wrapping CSV data
type CsvFile(filename:string) =

    let lines = 
      if filename.StartsWith("http") then
        use wc = new Net.WebClient()
        wc.DownloadString(Uri(filename)).Split([| '\r'; '\n' |], StringSplitOptions.RemoveEmptyEntries)
      else
        File.ReadAllLines(filename)

    // Cache the sequence of all data lines (all lines but the first)
    let data = 
        [ for line in lines do
           yield line.Split(',', ';') ]

    // Basic type inference
    let length = data |> Seq.head |> Seq.length
    let inv = System.Globalization.CultureInfo.InvariantCulture
    let inferType strings =      
      if strings |> Seq.forall (fun v -> 
        DateTime.TryParse(v, inv, Globalization.DateTimeStyles.None, ref DateTime.Now)) then DateTime
      elif strings |> Seq.forall (fun v ->
        Double.TryParse(v, Globalization.NumberStyles.AllowDecimalPoint, inv, ref 0.0)) then Float
      else String

    // Infer all types
    let types =
      [| for i in 0 .. length - 1 ->
          data |> Seq.skip 1 |> Seq.map (fun a -> Array.get a i) |> inferType |]

    member x.Headers = data |> Seq.head
    member x.Data = data |> Seq.skip 1 |> Seq.map (fun v -> CsvRow v)
    member x.Types = types

[<TypeProvider>]
type public CsvProvider(cfg:TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces()

    // Get the assembly and namespace used to house the provided types
    let asm = System.Reflection.Assembly.GetExecutingAssembly()
    let ns = "Samples.FSharp.CsvProvider"

    // Create the main provided type
    let csvTy = ProvidedTypeDefinition(asm, ns, "MiniCsv", Some(typeof<obj>))

    // Parameterize the type by the file to use as a template
    let filename = ProvidedStaticParameter("filename", typeof<string>)
    do csvTy.DefineStaticParameters([filename], fun tyName [| :? string as filename |] ->

        // resolve the filename relative to the resolution folder
        let resolvedFilename = 
          if filename.StartsWith("http") then filename
          else Path.Combine(cfg.ResolutionFolder, filename)
        
        // get the first line from the file
        let file = CsvFile(resolvedFilename) 

        // define a provided type for each row, erasing to a float[]
        let rowTy = ProvidedTypeDefinition("Row", Some(typeof<CsvRow>))

        // add one property per CSV field
        for i in 0 .. file.Headers.Length - 1 do
            let headerText, typ = file.Headers.[i], file.Types.[i]
            
            // try to decompose this header into a name and unit
            let prop =
                match typ with
                | String -> ProvidedProperty(headerText, typeof<string>, GetterCode = fun [row] -> 
                               <@@ (%%row:CsvRow).GetString(i) @@>)
                | DateTime -> ProvidedProperty(headerText, typeof<DateTime>, GetterCode = fun [row] -> 
                               <@@ (%%row:CsvRow).GetDateTime(i) @@>)
                | Float -> ProvidedProperty(headerText, typeof<float>, GetterCode = fun [row] -> 
                               <@@ (%%row:CsvRow).GetDouble(i) @@>)

            rowTy.AddMember(prop)
                
        // define the provided type, erasing to CsvFile
        let ty = ProvidedTypeDefinition(asm, ns, tyName, Some(typeof<CsvFile>))

        // add a parameterless constructor which loads the file that was used to define the schema
        ty.AddMember(ProvidedConstructor([], InvokeCode = fun [] -> <@@ CsvFile(resolvedFilename) @@>))

        // add a constructor taking the filename to load
        ty.AddMember(ProvidedConstructor([ProvidedParameter("filename", typeof<string>)], InvokeCode = fun [filename] -> <@@ CsvFile(%%filename) @@>))
        
        // add a new, more strongly typed Data property (which uses the existing property at runtime)
        ty.AddMember(ProvidedProperty("Data", typedefof<seq<_>>.MakeGenericType(rowTy), GetterCode = fun [csvFile] -> <@@ (%%csvFile:CsvFile).Data @@>))

        // add the row type as a nested type
        ty.AddMember(rowTy)
        ty)

    // add the type to the namespace
    do this.AddNamespace(ns, [csvTy])

[<TypeProviderAssembly>]
do()
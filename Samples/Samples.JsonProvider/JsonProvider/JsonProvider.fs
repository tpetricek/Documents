namespace FSharp.Json.ProviderImplementation

open System
open System.IO
open System.Linq.Expressions
open System.Reflection
open System.Globalization
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Samples.FSharp.ProvidedTypes

open FSharp.Net
open FSharp.Web
open FSharp.Web.JsonUtils
open FSharp.ProviderImplementation

// ----------------------------------------------------------------------------------------------

[<TypeProvider>]
type public JsonProvider(cfg:TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces()

  // Generate namespace and type 'FSharp.Web.JsonProvider'
  let asm = System.Reflection.Assembly.GetExecutingAssembly()
  let ns = "FSharp.Web"
  let jsonProvTy = ProvidedTypeDefinition(asm, ns, "JsonProvider", Some(typeof<obj>))

  let buildTypes (typeName:string) (args:obj[]) =

    // Generate the required type with empty constructor
    let resTy = ProvidedTypeDefinition(asm, ns, typeName, Some(typeof<obj>))
    ProvidedConstructor([], InvokeCode = fun _ -> <@@ new obj() @@>)
    |> resTy.AddMember

    // A type that is used to hide all generated domain types
    let domainTy = ProvidedTypeDefinition("DomainTypes", Some(typeof<obj>))
    resTy.AddMember(domainTy)


    // Infer the schema from a specified file or URI sample
    let sample = Helpers.readFileInProvider cfg (args.[0] :?> string)
    let infered = 
      let sampleList = args.[1] :?> bool
      if not sampleList then
        JsonInference.inferTypeFromCollection [JsonParser.parse sample]
      else
        let samples = [ for itm in JsonParser.parse sample -> itm ]
        JsonInference.inferTypeFromCollection samples


    // Generate static Parse method
    let ctx = JsonGenerationContext.Create(domainTy)
    let methResTy, methResConv = JsonTypeBuilder.generateJsonType ctx infered
    let args =  [ ProvidedParameter("source", typeof<seq<char>>) ]
    let m = ProvidedMethod("Parse", args, methResTy)
    m.IsStaticMethod <- true
    m.InvokeCode <- fun (Singleton source) -> methResConv <@@ JsonDocument.Create(JsonParser.parse (%%source)) @@>
    resTy.AddMember(m)

    // Return the generated type
    resTy

  // Add static parameter that specifies the API we want to get (compile-time) 
  let parameters = 
    [ ProvidedStaticParameter("SchemaUri", typeof<string>)
      ProvidedStaticParameter("SampleList", typeof<bool>, parameterDefaultValue = false) ]
  do jsonProvTy.DefineStaticParameters(parameters, buildTypes)

  // Register the main type with F# compiler
  do this.AddNamespace(ns, [ jsonProvTy ])

[<assembly:TypeProviderAssembly>]
do()
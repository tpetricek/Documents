namespace CultureProvider.TypeProvider

open System
open System.IO
open System.Linq.Expressions
open System.Reflection
open System.Globalization
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Samples.FSharp.ProvidedTypes

// -----------------------------------------------------------------

[<TypeProvider>]
type public DemoProvider(cfg:TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces()

  // Generate namespace and type 'CultureProvider.Cultures'
  let asm = System.Reflection.Assembly.GetExecutingAssembly()
  let ns = "CultureProvider"
  let sampleTy = ProvidedTypeDefinition(asm, ns, "Cultures", Some(typeof<obj>))

  let generateProperties() = 
    for culture in CultureInfo.GetCultures(CultureTypes.AllCultures) do
      let id = culture.Name
      ProvidedProperty
        ( culture.DisplayName, typeof<CultureInfo>,  
          IsStatic = true,
          GetterCode = fun args ->
            <@@ CultureInfo.GetCultureInfo(id) @@>)
      |> sampleTy.AddMember

  // Register the main type with F# compiler
  do
    generateProperties() 
    this.AddNamespace(ns, [ sampleTy ])

[<assembly:TypeProviderAssembly>]
do()
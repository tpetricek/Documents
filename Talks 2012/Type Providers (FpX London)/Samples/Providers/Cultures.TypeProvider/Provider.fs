namespace CultureProvider.TypeProvider

open System
open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Core.CompilerServices
open Samples.FSharp.ProvidedTypes

// -----------------------------------------------------------------
// Runtime components - used by the compiled code

module CultureRuntime =
  open System.Globalization

  let getCulture (name:string) = 
    CultureInfo.GetCultureInfo name

  let getAllCultures () =
    CultureInfo.GetCultures(CultureTypes.AllCultures)

// -----------------------------------------------------------------
// Compile-time components - culture type provider

[<TypeProvider>]
type public DemoProvider(cfg:TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces()
  do
    // Generate namespace and type 'CultureProvider.Cultures'
    let asm = System.Reflection.Assembly.GetExecutingAssembly()
    let ns = "CultureProvider"
    let sampleTy = ProvidedTypeDefinition(asm, ns, "Cultures", Some(typeof<obj>))

    // Add static property for every supported culture 
    for culture in CultureRuntime.getAllCultures() do
      let id = culture.Name
      ProvidedProperty
        ( culture.DisplayName, typeof<System.Globalization.CultureInfo>,  
          IsStatic = true,
          GetterCode = fun args ->
            <@@ CultureRuntime.getCulture id @@>)
      |> sampleTy.AddMember

    // Register the main type with F# compiler
    this.AddNamespace(ns, [ sampleTy ])

[<assembly:TypeProviderAssembly>]
do()
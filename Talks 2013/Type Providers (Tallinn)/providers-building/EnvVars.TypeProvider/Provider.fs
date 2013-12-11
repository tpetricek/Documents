namespace EnvVars.TypeProvider

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
type public EnvVarsProvider(cfg:TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces()

  // Generate namespace and type 'CultureProvider.Cultures'
  let asm = System.Reflection.Assembly.GetExecutingAssembly()
  let ns = "EnvVars"
  let sampleTy = ProvidedTypeDefinition(asm, ns, "Variables", Some(typeof<obj>))

  let generateProperties() = 
    ProvidedProperty
      ( "PATH", typeof<string>, IsStatic = true,
        GetterCode = fun args ->
        <@@ System.Environment.GetEnvironmentVariable("PATH") @@>)
    |> sampleTy.AddMember

  // Register the main type with F# compiler
  do
    generateProperties() 
    this.AddNamespace(ns, [ sampleTy ])

[<assembly:TypeProviderAssembly>]
do()
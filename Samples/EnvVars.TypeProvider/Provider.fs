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

// ----------------------------------------------------------------------------
// Start with some boiler-plate code that is same for pretty much all type
// providers - we need to define type marked as TypeProvider. The easiest way
// to build one is to inherit from TypeProviderForNamespaces (defined in 
// ProvidedTypes-0.2.fs/fsi (which is a sample from the F# team)

[<TypeProvider>]
type public EnvVarsProvider(cfg:TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces()

  // When the provider is instantiated, we generate all the
  // provided types and specify how to lazily load members

  // Generate namespace 'EnvVars' containing a type 'Variables'
  // (The bit about assembly is boilerplate code)
  let asm = System.Reflection.Assembly.GetExecutingAssembly()
  let ns = "EnvVars"
  let sampleTy = ProvidedTypeDefinition(asm, ns, "Variables", Some(typeof<obj>))

  // Specify how members are added (when they are requested)
  do sampleTy.AddMembersDelayed(fun () ->
    // Sequence expression that iterates over all environment
    // variables and generates property with the name of the variable
    // The GetterCode (executed when the code reading the property actually 
    // runs) simply reads the current value (this may fail if the variable
    // existed on a machine where the code was compiled, but does not
    // exist on a machine where it runs)
    [ for key in System.Environment.GetEnvironmentVariables().Keys do
        let name = unbox<string> key 
        let prop = ProvidedProperty( name, typeof<string>, IsStatic = true)
        prop.GetterCode <- fun args ->
          <@@ System.Environment.GetEnvironmentVariable(name) @@>
        yield prop ])

  // Register the main type with F# compiler
  do this.AddNamespace(ns, [ sampleTy ])

[<assembly:TypeProviderAssembly>]
do()
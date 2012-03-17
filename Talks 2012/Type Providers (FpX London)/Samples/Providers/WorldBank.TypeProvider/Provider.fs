namespace WorldBank.TypeProvider

open System
open System.Reflection
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations



type Runtime =
    static member SingleDimension(arg:string) : obj = 
        box arg
    // note
    static member GetCountriesByRegion(code:string) : seq<string> =
        seq { for (key, _) in WorldBank.GetCountries(code) -> key }

    static member GetByCountryIndicator(country:string, indicator:string) : seq<int * float> = 
        let data = 
          WorldBank.GetData ["countries"; country; "indicators"; indicator ] 
                            [ "date", "1960:2020" ] "date"
        seq { for (k, v) in data do
                if not (String.IsNullOrEmpty v) then 
                  yield int k, float v }

[<TypeProvider>]
type public WorldBankProvider() as this = 

    inherit TypeProviderForNamespaces()

    let thisAssembly = System.Reflection.Assembly.GetExecutingAssembly()
    let rootNamespace = "WorldBank" 
    let internalNamespace = "WorldBank.Internal" 

    let indicatorsType =
        let t = ProvidedTypeDefinition(thisAssembly, internalNamespace, "Indicators", baseType=Some typeof<obj>)
        t.AddMembersDelayed (fun () -> 
            [ for (id, name, note) in WorldBank.Indicators.Value do
                let prop = 
                  ProvidedProperty
                    ( name, typeof<seq<int * float>>, IsStatic=false,
                      GetterCode = (fun args -> 
                        Quotations.Expr.Coerce(<@@ Runtime.GetByCountryIndicator(unbox %%(Seq.head args), id) @@>, typeof<seq<int * float>>))) 
                if String.IsNullOrEmpty note |> not then prop.AddXmlDoc(note)
                yield prop ] )
        t

    let countriesType =
        let t = ProvidedTypeDefinition(thisAssembly, rootNamespace, "Countries", baseType=Some typeof<obj>)
        t.AddMembersDelayed (fun () -> 
            [ for (id, name) in WorldBank.Countries.Value ->
                ProvidedProperty
                  ( name, indicatorsType, IsStatic=true,
                    GetterCode = (fun _args -> 
                      Quotations.Expr.Coerce(<@@ Runtime.SingleDimension(id) @@>, typeof<string>))) ])
        t

    let indicatorsSeqType = typedefof<seq<_>>.MakeGenericType [| indicatorsType :> System.Type |]
    let regionsType =
        let t = ProvidedTypeDefinition(thisAssembly, rootNamespace, "Regions", baseType=Some typeof<obj>)
        t.AddMembersDelayed (fun () -> 
            [ for (code, name) in WorldBank.Regions.Value ->
                ProvidedProperty
                  ( name, indicatorsSeqType, IsStatic=true,
                    GetterCode = (fun _args -> 
                      Quotations.Expr.Coerce(<@@ Runtime.GetCountriesByRegion(code) @@>, indicatorsSeqType))) ])
        t
  
    do 
      this.AddNamespace(rootNamespace, [ countriesType; regionsType ])
      this.AddNamespace(internalNamespace, [ indicatorsType ])

[<assembly:TypeProviderAssembly>]
do()


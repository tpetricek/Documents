namespace FSharp.ProviderImplementation

open FSharp.Web
open FunJS

// ----------------------------------------------------------------------------------------------
// Runtime components used by the generated JSON types
// ----------------------------------------------------------------------------------------------

module List = 
  let emptyIfNull (l:'T list) = 
    if System.Object.Equals(Unchecked.defaultof<'T list>, l) then [] else l

type ContextHolder = 
  abstract Context : obj with get, set

type JsonDocument private (json:JSON) =
  member x.JSON = json
  override x.ToString() = json.ToString()

  static member Create(json:JSON) =
    JsonDocument(json)

  interface ContextHolder with
    member val Context : obj = null with get, set

type JsonOperations = 
  static member GetText(doc:JsonDocument) = 
    match doc.JSON with JsonText t -> t | _ -> failwith "JSON mismatch: Expected Text node"
  static member GetFloat(doc:JsonDocument) = 
    match doc.JSON with JsonNumber t -> t | _ -> failwith "JSON mismatch: Expected Number node"
  static member GetBoolean(doc:JsonDocument) = 
    match doc.JSON with JsonBoolean t -> t | _ -> failwith "JSON mismatch: Expected Boolean node"
  static member GetInt(doc:JsonDocument) = 
    match doc.JSON with JsonNumber t -> int t | _ -> failwith "JSON mismatch: Expected Number node"

  static member GetSingleByTypeTag(doc:JsonDocument, tag:int) = 
    let tag = enum<InferedTypeTag> tag
    let matchesTag = function
      | JsonArray _ -> tag = InferedTypeTag.Collection
      | JsonBoolean _ -> tag = InferedTypeTag.Boolean
      | JsonNull -> false
      | JsonNumber _ -> tag = InferedTypeTag.Number
      | JsonObject _ -> tag = InferedTypeTag.Object
      | JsonText _ -> tag = InferedTypeTag.String
    match doc.JSON with
    | JsonArray ar ->
        match ar |> List.filter matchesTag with
        | [the] -> JsonDocument.Create(the)
        | _ -> failwith "JSON mismatch: Expected single node, but found multiple"
    | _ -> failwith "JSON mismatch: Expected Array node"
  
  static member GetProperty(doc:JsonDocument, name) = 
    match doc.JSON with JsonObject m -> JsonDocument.Create(m.[name]) | _ -> failwith "JSON mismatch: Expected Object node"

  static member ConvertArray(doc:JsonDocument, f) = 
    match doc.JSON with 
    | JsonArray t -> [| for v in t -> f (JsonDocument.Create(v)) |]
    | _ -> failwith "JSON mismatch: Expected Array node"

  static member TryConvertProperty(doc:JsonDocument, name, f) = 
    match doc.JSON with 
    | JsonObject o -> 
        match o.TryFind name with
        | None | Some JsonNull -> None
        | Some it -> Some (f (JsonDocument.Create(it)))
    | _ -> None

// ----------------------------------------------------------------------------------------------
// Compile-time components that are used to generate JSON types
// ----------------------------------------------------------------------------------------------

open System
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Quotations
open FSharp.ProviderImplementation.Helpers

type internal JsonGenerationContext =
  { DomainType : ProvidedTypeDefinition
    UniqueNiceName : string -> string }
  static member Create(domainTy) =
    { DomainType = domainTy
      UniqueNiceName = NameUtils.uniqueNiceNameGenerator() }

module internal JsonTypeBuilder = 

  let rec generateJsonType ctx = function
    | InferedType.Primitive typ -> 
        let conv = 
          if typ = typeof<float> then fun json -> <@@ JsonOperations.GetFloat(%%json) @@>
          elif typ = typeof<int> then fun json -> <@@ JsonOperations.GetInt(%%json) @@>
          elif typ = typeof<string> then fun json -> <@@ JsonOperations.GetText(%%json) @@>
          elif typ = typeof<bool> then fun json -> <@@ JsonOperations.GetBoolean(%%json) @@>
          else failwith "Unsupported primitive type"
        typ, conv

    | InferedType.Bottom 
    | InferedType.Top -> 
        typeof<JsonDocument>, fun json -> <@@ (%%json:JsonDocument) @@>

    | InferedType.Collection typ -> 
        let elementTy, elementConv = generateJsonType ctx typ

        // Given 'js' of type JSON, we return conversion code:
        //   JsonConversions.ConvertArray<'T>(js, fun x -> %%(elementConv x))
        // where type 'T is the return type of the body of 'elementConv'
        let lambdaVar = Var.Global("x", typeof<JsonDocument>)
        let convBody = elementConv (Expr.Var lambdaVar)
        let conva = typeof<JsonOperations>.GetMethod("ConvertArray").MakeGenericMethod [| convBody.Type |]
        let convFunc = Expr.Lambda(lambdaVar, convBody)
        let conv = fun json -> Expr.Call(conva, [json; convFunc])
        elementTy.MakeArrayType(), conv

        // We're returning an array, but if we were returning sequence, use:
        //  typedefof<seq<_>>.MakeGenericType [| elementTy |], conv

    | InferedType.Heterogeneous types ->
        // Generate new type for the nested module
        let objectTy = ProvidedTypeDefinition(ctx.UniqueNiceName "Choice", Some(typeof<JsonDocument>))
        ctx.DomainType.AddMember(objectTy)
        
        // Add properties for getting nested things
        for (KeyValue(kindEnum, (multiplicity, typ))) in types do
          let kind = int kindEnum
          if multiplicity = Multiple then
            failwith "Multiple Multiplicity in Heterogeneous type not supported (yet)!"

          let valTy, valConv = generateJsonType ctx typ
          let p = ProvidedMethod("Get" + kindEnum.ToString(), [], valTy)
          p.InvokeCode <- fun (Singleton json) -> 
            valConv <@@ JsonOperations.GetSingleByTypeTag(%%json, kind) @@>
          objectTy.AddMember(p)          

        objectTy :> Type, fun json -> <@@ (%%json:JsonDocument) @@>

    | InferedType.Object props -> 
        // Generate new type for the nested module
        let objectTy = ProvidedTypeDefinition(ctx.UniqueNiceName "Entity", Some(typeof<JsonDocument>))
        ctx.DomainType.AddMember(objectTy)

        // Add properties
        for prop in props do
          let propName = prop.Name
          let propTy, getter =
            if not prop.Optional then 
              let valTy, valConv = generateJsonType ctx prop.Type
              valTy, fun (Singleton json) -> valConv <@@ JsonOperations.GetProperty(%%json, propName) @@>
            else
              let valTy, valConv = generateJsonType ctx prop.Type
              let optValTy = typedefof<option<_>>.MakeGenericType [| valTy |]

              // Similar to handling of arrays
              let lambdaVar = Var.Global("x", typeof<JsonDocument>)
              let convBody = valConv (Expr.Var lambdaVar)
              let conva = typeof<JsonOperations>.GetMethod("TryConvertProperty").MakeGenericMethod [| convBody.Type |]
              let convFunc = Expr.Lambda(lambdaVar, convBody)
              let conv = fun (Singleton json) -> Expr.Call(conva, [json; Expr.Value propName; convFunc])
              optValTy, conv

          let p = ProvidedProperty(NameUtils.niceName prop.Name, propTy)
          p.GetterCode <- getter
          objectTy.AddMember(p)          

        objectTy :> Type, fun json -> <@@ (%%json:JsonDocument) @@>

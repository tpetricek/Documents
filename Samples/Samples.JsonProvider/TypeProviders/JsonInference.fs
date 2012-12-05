namespace FSharp.Web

open System
open System.Collections.Generic
open FSharp.Web

// ------------------------------------------------------------------------------------------------

module Seq = 
  /// Merge two sequences by pairing elements for which
  /// the specified predicate returns the same key
  let pairBy f first second = 
    let d1 = dict [ for o in first -> f o, o ]
    let d2 = dict [ for o in second -> f o, o ]
    let keys = set (Seq.concat [ d1.Keys; d2.Keys ])
    let asOption = function true, v -> Some v | _ -> None
    [ for k in keys -> 
        k, asOption (d1.TryGetValue(k)), asOption (d2.TryGetValue(k)) ]
        
// ------------------------------------------------------------------------------------------------
// Infers the structure of JSON file from data
// ------------------------------------------------------------------------------------------------

type InferedProperty =
  { Name : string
    Optional : bool
    Type : InferedType }

and InferedMultiplicity = 
  | Single
  | Multiple 

and InferedTypeTag = 
  | Number = 1
  | Boolean = 2 
  | Object = 3 
  | Collection = 4
  | Bottom = 5
  | String = 6

and InferedType =
  | Primitive of System.Type
  | Object of seq<InferedProperty>
  | Collection of InferedType
  | Heterogeneous of Map<InferedTypeTag, InferedMultiplicity * InferedType>
  | Top
  | Bottom
  member x.TypeTag = 
    match x with
    | Object _ -> InferedTypeTag.Object
    | Collection _ -> InferedTypeTag.Collection
    | Top -> InferedTypeTag.Bottom
    | Bottom -> InferedTypeTag.Bottom
    | Heterogeneous _ -> InferedTypeTag.Bottom
    | Primitive typ when typ = typeof<int> || typ = typeof<float> ->  
        InferedTypeTag.Number
    | Primitive typ when typ = typeof<bool> ->  
        InferedTypeTag.Boolean
    | Primitive typ when typ = typeof<string> ->  
        InferedTypeTag.String
    | _ -> failwith "Cannot get type tag of '%A'" x

// ------------------------------------------------------------------------------------------------

module JsonInference = 

  /// Get the union of two types
  let rec unionTypes ot1 ot2 =
    match ot1, ot2 with
    // Primitive types: int can be converted to float, otherwise have to match
    | Primitive t1, Primitive t2 when t1 = t2 -> Primitive t1
    | Primitive t1, Primitive t2 when 
        (t1 = typeof<float> && t2 = typeof<int>) ||
        (t2 = typeof<float> && t1 = typeof<int>) -> Primitive typeof<float>    
    | Primitive t1, Primitive t2 -> Primitive typeof<string>

    // Union types of a list or properties of complex type
    | Collection t1, Collection t2 -> Collection (unionTypes t1 t2)
    | Object t1, Object t2 -> Object (unionObjectTypes t1 t2)
    
    // Top type (null) can be merged with anything else
    // but two incompatible types result in Bottom (object)
    | t, Top | Top, t -> t

    // Two heterogeneous things just get merged
    | _ -> Bottom

  /// Get the union of object types (merge their properties)
  and unionObjectTypes t1 t2 =
    Seq.pairBy (fun p -> p.Name) t1 t2
    |> Seq.map (fun (name, fst, snd) ->
        match fst, snd with
        // If one is missing, return the other, but optional
        | Some p, None | None, Some p -> { p with Optional = true }
        // If both are available, we merge their types
        | Some p1, Some p2 -> 
            // If any of the arguments is Top, it means we may get null value
            let anyNull = p1.Type = Top || p2.Type = Top
            { Name = name; Optional = p1.Optional || p2.Optional || anyNull
              Type = unionTypes p1.Type p2.Type }
        | _ -> failwith "unionObjectTypes: pairBy returned None, None")

  /// Infer type of a single JSON sample value
  let rec inferType = function
    | JsonText _ -> Primitive typeof<string>
    | JsonNumber n when n = Math.Round n -> Primitive typeof<int>
    | JsonNumber _ -> Primitive typeof<float>
    | JsonBoolean _ -> Primitive typeof<bool>
    | JsonNull -> Top
    | JsonArray ar -> inferCollectionOrHeterogeneous ar
    | JsonObject o ->
        let props = 
          [ for (KeyValue(k, v)) in o -> 
              { Name = k; Optional = false; Type = inferType v } ]
        Object props

  /// Infer type from multiple examples of JSON values
  and inferTypeFromCollection (values:seq<JSON>) =
    let heterogeneousTypes =
      [ for v in values -> inferType v ]
      |> Seq.groupBy (fun typ -> typ.TypeTag)
      |> Seq.map (fun (tag, group) -> 
          match List.ofSeq group with
          | [single] -> tag, (Single, single)
          | multiple -> tag, (Multiple, group |> Seq.fold unionTypes Top))
      |> List.ofSeq
    match heterogeneousTypes with
    | [] -> Top
    | [_, (_, single)] -> single
    | types -> Heterogeneous(Map.ofSeq types)

  and inferCollectionOrHeterogeneous values = 
    let typ = inferTypeFromCollection values 
    match typ with
    | Heterogeneous _ -> typ
    | _ -> Collection typ

// ------------------------------------------------------------------------------------------------

module Tests = 
  let ``Pair by helper function works``() = 
    let actual = Seq.pairBy fst [(1, "a"); (2, "b")] [(1, "A"); (3, "C")]
    let expected = 
      [ (1, Some (1, "a"), Some (1, "A"))
        (2, Some (2, "b"), None)
        (3, None, Some (3, "C")) ]
    actual = expected

  let ``Merge primitive types into a string``() =
    let source = JsonParser.parse """[ true, 1, "hi" ]"""
    let expected = Collection(Primitive typeof<string>)
    let actual = JsonInference.inferType source
    actual = expected

  let ``Merge primitive types and objects into bottom type``() =
    let source = JsonParser.parse """[ true, 1, "hi", { "a": true } ]"""
    let expected = Collection(Bottom)
    let actual = JsonInference.inferType source
    actual = expected

  let ``Merge types in a collection of collections``() =
    let source = JsonParser.parse """[ [{"a":true},{"b":1}], [{"b":1.1}] ]"""
    let expected = 
      Object [ {Name = "a"; Optional = true; Type = Primitive typeof<bool> }
               {Name = "b"; Optional = true; Type = Primitive typeof<bool> } ]
      |> Collection |> Collection
    let actual = JsonInference.inferType source
    actual = expected

  let ``Union properties of a collection and unify float and int``() =
    let source = JsonParser.parse """[ {"a":1, "b":""}, {"a":1.2, "c":true} ]"""
    let expected =
      Object [ {Name = "a"; Optional = false; Type = Primitive typeof<float> }
               {Name = "b"; Optional = true; Type = Primitive typeof<string> }
               {Name = "c"; Optional = true; Type = Primitive typeof<bool> }]
      |> Collection
    let actual = JsonInference.inferType source
    actual = expected

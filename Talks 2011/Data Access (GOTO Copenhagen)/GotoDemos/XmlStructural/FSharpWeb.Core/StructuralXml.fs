// --------------------------------------------------------------------------------------
// Simple API for working with XML based on matching data 
// to a structure defined using F# discriminated unions
// --------------------------------------------------------------------------------------

namespace FSharpWeb.Core
open System
open System.Net
open System.Xml.Linq

open Microsoft.FSharp.Reflection

// --------------------------------------------------------------------------------------

module Internal = 
  // Type to hold a generic method (for easy access using reflection)
  type Helper = 
    static member CastList<'T>(input:System.Collections.IEnumerable) =
      input |> Seq.cast<'T> |> List.ofSeq  

  /// Converts any 'IEnumerable' to a list 'list<#type>' where
  /// type is specified as an argument System.Type
  let castList typ input = 
    let mi = typeof<Helper>.GetMethod("CastList").MakeGenericMethod [| typ |]
    mi.Invoke(null, [| input |])


// --------------------------------------------------------------------------------------

/// Provides an easy access to XML data
type StructuralXml<'T> private (url:string, ns, lowerCase) = 

  /// A name resolver that turns member name into XName
  /// depending on the class configuration (namespace, lowerCase flag)
  let resolveName (str:string) = 
    let str = if lowerCase then str.ToLower() else str
    match ns with 
    | Some(ns) -> XName.Get(str, ns)
    | _ -> XName.Get(str)
    

  /// 
  let rec parseType (element:XContainer) (targetType:System.Type) = 

    // Determine information about the target type
    // If it is list, the 'typ' is the element type.
    let isList, typ = 
      if targetType.IsGenericType && 
         targetType.GetGenericTypeDefinition() = typedefof<_ list> then 
        true, targetType.GetGenericArguments().[0]
      else false, targetType
  
    if typ = typeof<string> then
      // When target is 'string', get the XElement's body
      box (element :?> XElement).Value
    elif not(FSharpType.IsUnion(typ)) then 
      // When it's not a discriminated union, then that's error
      failwithf "Expected discriminated union!\nGot: %s" typ.Name
    else
      // For every union case, find all children matching the case name
      let children =
        [ for case in FSharpType.GetUnionCases(typ) do
            let fields = case.GetFields()
            let children = element.Elements(resolveName case.Name)
            for ch in children do 
              // Recursively parse children and match them to the required type
              let args = [| for field in fields -> parseType ch field.PropertyType |]
              yield FSharpValue.MakeUnion(case, args) ]

      // If the result is list, convert it to the right type.
      // If it's not a list, return the child as object.
      match isList, children with
      | true, children -> Internal.castList typ children
      | false, [child] -> child
      | false, _ -> 
          // When expected type is non-list, but we find
          // multiple children, that's an error...
          failwithf 
            "Wrong number of children in node (%d).\nWhen formatting XML as '%s'." 
            children.Length typ.Name


  // Parse the document & store it in a local field
  let root : 'T = (parseType (XDocument.Load(url)) typeof<'T>) :?> 'T

  // ------------------------------------------------------------------------------------

  /// Returns the parsed XML data structure as a value of the user-specified type
  member x.Root = root

  /// Load XML data from the specified URI and dynamically match them
  /// to a structure described by the discriminated union 'T. Optional
  /// arguments can be used to specify default XML namespace and to 
  /// specify that case names should be treated as lower case.
  static member Load<'T>(url, ?Namespace, ?LowerCase) = 
    new StructuralXml<'T>(url, Namespace, defaultArg LowerCase false)



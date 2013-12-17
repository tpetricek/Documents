namespace ProviderImplementation

open Microsoft.FSharp.Core.CompilerServices
open Samples.FSharp.ProvidedTypes
open Microsoft.FSharp.Data.TypeProviders
open Microsoft.FSharp.Quotations

// ----------------------------------------------------------------------------
// Runtime components, used by both the type provider ("desing time") and
// by the generated code ("run time") - handles getting data & in-memory caching
// ----------------------------------------------------------------------------

/// Using WSDL type provider to communicate with Tesco
type Tesco = WsdlService<"http://www.techfortesco.com/groceryapi/soapservice.asmx">

module TescoRuntime = 
  // Generic memoization of a function
  let memoize f = 
    let cache = System.Collections.Concurrent.ConcurrentDictionary<_, _>()
    fun input -> 
      if not (cache.ContainsKey(input)) then 
        cache.TryAdd(input, f input) |> ignore
      cache.[input]

  /// Returns active connection for a given user & password
  let getConnection = memoize (fun (user, pass) ->
    let client = Tesco.GetSOAPServiceSoap()
    let _, session = client.Login(user, pass, "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
    client, session )

  /// Get categories for a connection (given user credentials)
  let getCategories = memoize (fun credentials ->
    let client, session = getConnection credentials
    let _, categories = client.ListProductCategories(session)
    categories )

  /// Get mapping from IDs to category objects 
  let getAllCategories = memoize (fun credentials ->
    let rec loop (categories:seq<Tesco.ServiceTypes.www.techfortesco.com.Category>) = 
      [ for c in categories do
          yield c.Id, c
          if c.Children <> null then yield! loop c.Children ]
    let categories = getCategories credentials
    dict (loop categories) )

  /// Given credentials & category ID, find all products in that category
  /// (This is only used in runtime and so we do not cache it)
  let getProducts credentials categoryID = 
    let category = (getAllCategories credentials).[categoryID]
    let client, session = getConnection credentials
    let _, products = client.ListProductsByCategory(session, category, false)
    products

// ----------------------------------------------------------------------------
// Runtime-only types that are only used in generated code
// ----------------------------------------------------------------------------
  
/// Any Tesco runtime type that keeps credentials
type IConnection = 
  abstract Credentials : string * string

/// Type representing top-level object 
type Connection(credentials) = 
  interface IConnection with
    member x.Credentials = credentials

/// Type representing a category    
type Category(credentials, cateogryID) = 
  member x.CategoryID = cateogryID
  interface IConnection with
    member x.Credentials = credentials
  interface System.Collections.IEnumerable with 
    member x.GetEnumerator() = (x:>seq<_>).GetEnumerator() :> _
  interface System.Collections.Generic.IEnumerable<Tesco.ServiceTypes.www.techfortesco.com.Product> with
    member x.GetEnumerator() = 
      let prods = TescoRuntime.getProducts credentials cateogryID
      (prods :> seq<_>).GetEnumerator()

// ----------------------------------------------------------------------------
// Type provider
// ----------------------------------------------------------------------------

[<TypeProvider>]
type public TescoProvider(cfg:TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces()

  // Boilerplate that generates root type in current assembly
  let asm = System.Reflection.Assembly.GetExecutingAssembly()
  let ns = "Tesco"
  let tescoType = ProvidedTypeDefinition(asm, ns, "TescoProvider", Some(typeof<obj>))

  // Add static parameters that specify the credentials
  let parameters = 
    [ ProvidedStaticParameter("Username", typeof<string>)
      ProvidedStaticParameter("Password", typeof<string>) ]
  do tescoType.DefineStaticParameters(parameters, fun typeName args ->
    
    // Called by the compiler when the parameters are specified - get values
    let user, pass = args.[0] :?> string, args.[1] :?> string

    // Generate top-level type (which does not represent any category) 
    // Generate "DomainTypes" where we 'hide' all generate types
    let topTy = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, HideObjectMethods = true)
    let domainTy = ProvidedTypeDefinition("DomainTypes", Some typeof<obj>)
    topTy.AddMember(domainTy)

    // Generate tree-like structure for all categories
    // Given parent type & list of categories, add members for 
    // accessing all the categories (lazily!) to the type
    let rec generateCategories (parentTy:ProvidedTypeDefinition) (categories:seq<Tesco.ServiceTypes.www.techfortesco.com.Category>) =
      if categories <> null then
        parentTy.AddMembersDelayed(fun () ->
          [ for cat in categories do 
              // Generate type for category, add it to DomainTypes & add children lazily 
              let newTy = ProvidedTypeDefinition(cat.Name, Some typeof<Category>, HideObjectMethods = true)
              domainTy.AddMember(newTy)
              generateCategories newTy cat.Children
              // Add a property returning the generated type to the parent
              let prop = ProvidedProperty(cat.Name, newTy)
              // Store 'categoryID' in a local variable (this way, we can capture
              // it in a quotation later; cast self to IConnection to get credentials)
              let categoryID = cat.Id
              prop.GetterCode <- fun [self] -> 
                  let conn = Expr.Coerce(self, typeof<IConnection>)
                  <@@ let c : IConnection = %%conn in Category(c.Credentials, categoryID)  @@>
              yield prop ])

    // Add constructor so that we can create the top-level object    
    // (This is not entirely safe, because it captures user & password - 
    // in principle, we should probably ask for these again so that they
    // are not stored in the generated code - but whatever, it's a demo!)
    let ctor = ProvidedConstructor([])
    ctor.InvokeCode <- fun [] -> <@@ new Connection(user, pass) @@>
    generateCategories topTy (TescoRuntime.getCategories (user, pass))
    topTy.AddMember(ctor)
    topTy )
 
  // Register the main (parameterized) type with F# compiler
  do this.AddNamespace(ns, [ tescoType ])

[<assembly:TypeProviderAssembly>]
do()
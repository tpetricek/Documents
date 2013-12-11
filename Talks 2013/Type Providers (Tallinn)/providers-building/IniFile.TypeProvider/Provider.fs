namespace IniFile.TypeProvider

open System
open System.IO
open System.Linq.Expressions
open System.Reflection
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations
open System.Collections.Generic
open Samples.FSharp.ProvidedTypes

// -----------------------------------------------------------------
// At runtime, we represent INI file data using the following
// two types - IniFile represents an entire file (loaded by 
// constructing the provided type) and IniGroup represents group
// of data (result of accessing generated property with group name)
// -----------------------------------------------------------------

type IniGroup(values:IDictionary<string, string>) =
  member x.Values = values

type IniFile(groups:IDictionary<string, IniGroup>) =
  member x.Groups = groups

/// The following code implements parsing of INI files and returns
/// IniFile. At runtime, it is used to read the data. At compile
/// time, it is used to infer the structure.  
module IniParser = 
  /// Given a sequence of lines in a form 'key=value', 
  /// returns a key-value dictionary
  let readKeyValues (lines:seq<string>) : IniGroup = 
    let values = seq { 
      for l in lines do
        let idx = l.IndexOf('=')
        if idx > 0 then
          yield l.Substring(0, idx), l.Substring(idx + 1, l.Length - idx - 1).Trim('"') }
    IniGroup(dict values)

  /// Given a file name, returns IniFile containing several groups
  let readIniFile file : IniFile =
    let lines = File.ReadAllLines(file)
    let groups =
      Seq.concat [ [| "Default" |]; lines ]
      |> Seq.filter (fun l -> not (String.IsNullOrWhiteSpace(l)))
      |> Seq.map (fun l -> l.Trim())
      |> Seq.filter (fun l -> not (l.StartsWith(";")))
      |> Seq.groupAdjacent (fun l ->
          if l.StartsWith("[") && l.EndsWith("]") then  
            Some(l.Trim('[', ']')) else None)
      |> Seq.map (fun (k, lines) -> k, readKeyValues lines)
    
    IniFile(dict groups)


// -----------------------------------------------------------------
// Type provider generates a parameterized type 'IniFile' in 
// 'IniProvider' namespace. When provided with a file, it creates
// a type that can be constructed (and loads file) and exposes 
// groups using properties
// -----------------------------------------------------------------

[<TypeProvider>]
type public IniFileProvider(cfg:TypeProviderConfig) as this =
  inherit TypeProviderForNamespaces()

  // Generate namespace and type 'IniProvider.IniFile'
  let asm = System.Reflection.Assembly.GetExecutingAssembly()
  let ns = "IniProvider"
  let iniType = ProvidedTypeDefinition(asm, ns, "IniFile", Some(typeof<obj>))

  // Add static parameter that specifies the (compile-time) ini file
  let parameter = ProvidedStaticParameter("FileName", typeof<string>)
  do iniType.DefineStaticParameters([parameter], fun typeName args ->

    // When given file name (at compile-time), we parse the file and
    // generate types based on the group names & key names
    let fileName = args.[0] :?> string
    let resolvedFilename = Path.Combine(cfg.ResolutionFolder, fileName)
    let ini = IniParser.readIniFile resolvedFilename


    // -------------------------------------------------------------
    // Generating types - we generate type that represents the INI
    // file (with a constructor that reads the file) and with static
    // properties for every group in the file.    
    // -------------------------------------------------------------


    let resTy = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<IniFile>)
    ProvidedConstructor([], InvokeCode = fun _ -> 
      // The body of the constructor is an expression
      // that creates IniFile value (we could add another
      // constructor taking file name as parameter)
      <@@ 
        IniParser.readIniFile resolvedFilename 
      @@>)
    |> resTy.AddMember


    // SAMPLE CODE: The following snippet shows how to add a new type,
    // inherited from IniGroup, to the existing namespace (as a nested type).
    let sampleTy = ProvidedTypeDefinition("TestType", Some(typeof<IniGroup>))
    resTy.AddMember(sampleTy)

    ProvidedProperty("TestProp", typeof<string>, GetterCode = fun [self] ->
      <@@ "Not implemented" @@>)
    |> sampleTy.AddMember


    // For every group in the file (loaded at compile-time), we
    // add a new property that returns 'IniGroup'.
    for (KeyValue(groupName, items)) in ini.Groups do   
    
      // Generate property with the value of 'key' as the name. 
      // The body of the property performs lookup in the IniFile and
      // returns a value of IniGroup (for now) representing the current group.
      ProvidedProperty(groupName, typeof<IniGroup>, GetterCode = fun [self] ->
        <@@ 
          // 'self' represents the instance created in the constructor.
          // We return group with the specified key.
          (%%self:IniFile).Groups.[groupName] 
        @@>)
      |> resTy.AddMember

      // TODO: Change the type of the generated properties (representing
      // groups) from plain 'IniGroup' to a 'sampleTy' that inherits from
      // IniGroup, but adds an additional property 'TestProp'
      
      // TODO: Generate a new type (similar to sampleTy) for every group 
      // Then iterate over all the keys in 'items' and add them as properties
      // to this new type. The properties will get 'IniGroup' as 'self' and
      // can perform lookup using a given key.

    resTy)
 

  // Register the main (parameterized) type with F# compiler
  do this.AddNamespace(ns, [ iniType ])

[<assembly:TypeProviderAssembly>]
do()
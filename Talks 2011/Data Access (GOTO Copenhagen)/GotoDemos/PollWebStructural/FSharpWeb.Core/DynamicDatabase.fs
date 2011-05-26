// --------------------------------------------------------------------------------------
// Simple API for working with SQL Server using F# dynamic and Reflection
// --------------------------------------------------------------------------------------

namespace FSharpWeb.Core

open System.Data 
open System.Data.SqlClient
open Microsoft.FSharp.Reflection

// --------------------------------------------------------------------------------------

module Internal =
  /// Creates a function that fills F# record (specified by the type argument)
  /// with values from a SqlDataReader (using F# reflection magic)
  let createParser(typ) =
    let fields = FSharpType.GetRecordFields(typ)
    let ctor = FSharpValue.PreComputeRecordConstructor(typ)
    fun (reader:SqlDataReader) ->
      ctor [| for f in fields -> reader.[f.Name] |]

  type Helper = 
    static member CastSeq<'T>(input:System.Collections.IEnumerable) =
      input |> Seq.cast<'T>
  
  /// Converts any 'IEnumerable' to a sequence 'seq<#type>' where
  /// type is specified as an argument System.Type
  let castSeq typ input = 
    let mi = typeof<Helper>.GetMethod("CastSeq").MakeGenericMethod [| typ |]
    mi.Invoke(null, [| input |])

  /// Create a SQL command for calling stored procedure specified by 'name'
  /// with arguments specified by 'args' (which may be a tuple for multiple
  /// arguments). 
  let createCommand name (args:'T) connection = 
    let cmd = new SqlCommand(name, connection)
    cmd.CommandType <- CommandType.StoredProcedure
    
    // Get parameter list from the SQL server
    SqlCommandBuilder.DeriveParameters(cmd)
    // We're only interested in input parameters
    let parameters = 
      [| for (p:SqlParameter) in cmd.Parameters do
           if p.Direction = ParameterDirection.Input then yield p |]
    // Get arguments from the tuple (unit -> empty, tuple -> multiple)
    let arguments = 
      if typeof<'T> = typeof<unit> then [| |]
      elif FSharpType.IsTuple(typeof<'T>) then FSharpValue.GetTupleFields(args)
      else [| args |]

    // Match parameters with arguments & set the values
    if arguments.Length <> parameters.Length then 
      failwith "Incorrect number of arguments!"
    for (par, arg) in Seq.zip parameters arguments do 
      par.Value <- arg
    cmd

// --------------------------------------------------------------------------------------

type DynamicDatabase(connectionString:string) =
  member private x.ConnectionString = connectionString

  static member (?) (x:DynamicDatabase, name) : 'T -> 'R = fun (args:'T) -> 
    if typeof<'R> = typeof<unit> then
      // The result should be unit, so we execute it using 'non-query'
      use cn = new SqlConnection(x.ConnectionString)
      cn.Open()
      let cmd = Internal.createCommand name args cn
      cmd.ExecuteNonQuery() |> ignore
      () |> box |> unbox

    elif typeof<'R>.IsGenericType && typedefof<'R> = typedefof<seq<_>> then
      // The result should be a sequence of some types, so we execute
      // it using 'query' and then convert the results to the desired
      // type dynamically.
      let typ = typeof<'R>.GetGenericArguments().[0]
      let parser = Internal.createParser(typ)
      seq { use cn = new SqlConnection(x.ConnectionString)
            cn.Open()
            let cmd = Internal.createCommand name args cn
            let reader = cmd.ExecuteReader()
            while reader.Read() do yield parser(reader) }
      |> Internal.castSeq typ |> unbox

    else
      // The result type is something else, which is an error
      failwith "The result of '?' operator should be unit or a sequence!"

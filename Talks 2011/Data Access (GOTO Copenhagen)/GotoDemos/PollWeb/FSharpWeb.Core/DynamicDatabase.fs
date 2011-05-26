// ----------------------------------------------------------------------------------------
// This file is based on the sample from: http://tomasp.net/blog/dynamic-sql.aspx
// For a commented & improved version of this file, see the '3 Structural Database' sample
// ----------------------------------------------------------------------------------------

namespace FSharpWeb.Core

open System.Data 
open System.Data.SqlClient
open Microsoft.FSharp.Reflection

module Internal =
  let createCommand name (args:'T) connection = 
    let cmd = new SqlCommand(name, connection)
    cmd.CommandType <- CommandType.StoredProcedure
    SqlCommandBuilder.DeriveParameters(cmd)
    let parameters = 
      [| for (p:SqlParameter) in cmd.Parameters do
           if p.Direction = ParameterDirection.Input then
             yield p |]
    let arguments = 
      if typeof<'T> = typeof<unit> then [| |]
      elif FSharpType.IsTuple(typeof<'T>) then FSharpValue.GetTupleFields(args)
      else [| args |]
    if arguments.Length <> parameters.Length then 
      failwith "Incorrect number of arguments!"
    for (par, arg) in Seq.zip parameters arguments do 
      par.Value <- arg
    cmd

type DatabaseNonQuery(connectionString:string) = 
  member private x.ConnectionString = connectionString
  static member (?) (x:DatabaseNonQuery, name) = fun (args:'T) -> 
    use cn = new SqlConnection(x.ConnectionString)
    cn.Open()
    let cmd = Internal.createCommand name args cn
    cmd.ExecuteNonQuery() |> ignore

type Row(reader:SqlDataReader) = 
  member private x.Reader = reader
  static member (?) (x:Row, name:string) : 'R = 
    x.Reader.[name] :?> 'R

type DatabaseQuery(connectionString:string) = 
  member private x.ConnectionString = connectionString
  static member (?) (x:DatabaseQuery, name) = fun (args:'T) -> seq {
    use cn = new SqlConnection(x.ConnectionString)
    cn.Open()
    let cmd = Internal.createCommand name args cn
    let reader = cmd.ExecuteReader()
    while reader.Read() do
      yield Row(reader) }

type DynamicDatabase(connectionString:string) =
  member x.Query = DatabaseQuery(connectionString)
  member x.NonQuery = DatabaseNonQuery(connectionString)
  
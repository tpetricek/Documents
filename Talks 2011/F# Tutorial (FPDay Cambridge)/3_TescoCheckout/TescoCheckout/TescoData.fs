module Checkout.Products

open TescoApi.Tesco
open System.Configuration
open System.ServiceModel
open System.ServiceModel.Configuration

open Checkout

type SOAPServiceSoap with
  member x.AsyncLogin(request) = 
    Async.FromBeginEnd(request, x.BeginLogin, x.EndLogin)
  member x.AsyncProductSearch(request) =
    Async.FromBeginEnd(request, x.BeginProductSearch, x.EndProductSearch)



let Lookup code = 

  // Login to Tesco
  let client = new SOAPServiceSoapClient()
  let status, session = 
    client.Login("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
  
  let status, products, pageCount, prodCount = 
    client.ProductSearch(session, code, true, 1)

  if prodCount > 0 then
    let p = products.[0]
    Some(Product(p.EANBarcode, p.Name, p.ImagePath, decimal p.Price))
  else 
    None

let products = 
  [ Product("8717644012208","Lynx Africa","",2.49M)
    Product("5010123730215","Listerine","",2.49M)
    Product("5000347009242","AquaFresh","",1.99M)
    Product("5045094763863","Ibuprofen","", 1.99M)]

let BackupLookup code = 
  products |> Seq.tryFind (function Product(code',_,_,_) -> code' = code)
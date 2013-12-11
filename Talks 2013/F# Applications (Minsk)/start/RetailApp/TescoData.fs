module Checkout.Products

open System.Configuration
open System.ServiceModel
open System.ServiceModel.Configuration
open Microsoft.FSharp.Data
open Checkout.Domain

// ------------------------------------------------------------------
// Read data using WSDL type providers 

#if LIVE
type Tesco = TypeProviders.WsdlService<"http://www.techfortesco.com/groceryapi/soapservice.asmx">
#else
type Tesco = TypeProviders.WsdlService<"http://localhost:3353/TescoService.svc">
#endif
  
// ------------------------------------------------------------------
// Create value of type Product from Tesco data

// TODO: Make web service lookup asynchronous

#if LIVE 

let Lookup search = 
  let client = Tesco.GetSOAPServiceSoap()
  let _, session = client.Login("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
  let _, products, _, _ = client.ProductSearch(session, search, false, 1)
  match products |> List.ofSeq with
  | prod::_ -> Some(Product(prod.EANBarcode, prod.Name, prod.ImagePath, decimal prod.Price))
  | _ -> None

#else

let Lookup search = 
  let client = Tesco.GetBasicHttpBinding_ITescoService()
  let session = client.Login("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
  let! products = client.ProductSearchAsync(session, search) 
  match products |> List.ofSeq with
  | prod::_ -> return Some(Product(prod.EANBarcode, prod.Name, prod.ImagePath, decimal prod.Price))
  | _ -> return None }

#endif

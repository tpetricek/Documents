module Checkout.Products

open System.Configuration
open System.ServiceModel
open System.ServiceModel.Configuration

open Microsoft.FSharp.Data
open Checkout.Domain

//type Tesco = TypeProviders.WsdlService<"http://www.techfortesco.com/groceryapi/soapservice.asmx">
type Tesco = TypeProviders.WsdlService<"http://localhost:3353/TescoService.svc">


let Lookup search = 
  
  let client = Tesco.GetBasicHttpBinding_ITescoService()
  let session = client.Login("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
  let products = client.ProductSearch(session, search)
  match products |> List.ofSeq with
  | prod::_ -> Some(Product(prod.EANBarcode, prod.Name, prod.ImagePath, decimal prod.Price))
  | _ -> None

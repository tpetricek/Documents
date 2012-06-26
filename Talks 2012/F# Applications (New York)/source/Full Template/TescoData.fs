module Checkout.Products

open System.Configuration
open System.ServiceModel
open System.ServiceModel.Configuration
open Microsoft.FSharp.Data
open Checkout.Domain

type Tesco = TypeProviders.WsdlService<"http://localhost:3353/TescoService.svc">

let Lookup code = 
  let svc = Tesco.GetBasicHttpBinding_ITescoService()
  let session = svc.Login("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
  match svc.ProductSearch(session, code) |> List.ofSeq with
  | p::_ -> Some(Product(p.EANBarcode, p.Name, decimal p.Price, p.ImagePath))
  | _ -> None

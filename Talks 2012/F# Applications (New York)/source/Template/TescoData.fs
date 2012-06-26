module Checkout.Products

open System.Configuration
open System.ServiceModel
open System.ServiceModel.Configuration
open Microsoft.FSharp.Data
open Checkout.Domain

type Tesco = TypeProviders.WsdlService<"http://localhost:3353/TescoService.svc">

let Lookup code = async {
  let svc = Tesco.GetBasicHttpBinding_ITescoService()
  let! session = 
    svc.LoginAsync("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
    |> Async.AwaitTask
  let! prods = 
    svc.ProductSearchAsync(session, code)
    |> Async.AwaitTask
  match prods |> List.ofSeq with
  | prod::_ ->
      return Some(Product(prod.EANBarcode, prod.Name, decimal prod.Price, prod.ImagePath))
  | _ -> 
      return None }

module TescoApplication

type Name = string
type Price = decimal
type Code = string
type ImagePath = string

type Product = Product of Code * Name * Price * ImagePath

type Quantity = int

type LineItem = 
  | SaleLineItem of int * Product * Quantity
  | CancelItem of int

open Microsoft.FSharp.Data

type Tesco = TypeProviders.WsdlService<"http://localhost:3353/TescoService.svc">

let products =
  [ Product("32499021", "Biscuits", 1.50M, "N/A")
    Product("98234923", "Earl Gray Tea", 2.60M, "N/A")
    Product("9770013061206", "The Economist", 4.20M, "N/A") ]

let search code = 
  let svc = Tesco.GetBasicHttpBinding_ITescoService()
  let session = svc.Login("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
  [ for p in svc.ProductSearch(session, code) ->
      Product(p.EANBarcode, p.Name, decimal p.Price, p.ImagePath) ]
  |> List.head

type Sale = LineItem list

type FinalSale = Map<Product, int>

let completeSale (sale:Sale) = 
  let cancelled = 
    sale 
    |> Seq.choose (function
      | CancelItem n -> Some n
      | _ -> None) 
    |> set

  sale 
  |> Seq.choose (function 
    | SaleLineItem(n, prod, q) when not (cancelled.Contains(n)) ->
        Some(prod, q)
    | _ -> None)
  |> Seq.groupBy fst
  |> Seq.map (fun (prod, agg) ->
        prod, agg |> Seq.map snd |> Seq.sum)
  |> Map.ofSeq

open System

let input = Console.ReadLine()
let prod = search input
printfn "%A" prod


open NUnit.Framework

[<TestFixtureAttribute>]
module SaleTests =
  [<TestAttribute>]
  let ``Calculating sale removes cancelled``() =
    let fin =
      [ SaleLineItem(1, search "9770013061206", 2)
        CancelItem(1)
        SaleLineItem(2, search "98234923", 1)
        SaleLineItem(2, search "98234923", 3) ]
      |> completeSale
    Assert.That((fin = Map.ofSeq [ search "98234923", 4 ]))

//  do ``Calculating sale removes cancelled``()

namespace Checkout.Domain

type Name = string
type Price = decimal
type Code = string
type ImagePath = string

type Product = Product of Code * Name * Price * ImagePath

type Quantity = int

type LineItem = 
  | SaleLineItem of int * Product * Quantity
  | CancelItem of int

type Sale = LineItem list

type FinalSale = Map<Product, int>

module SaleOperations =
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

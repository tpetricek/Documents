namespace Checkout.Domain

type Code = string
type Name = string
type Price = decimal
type ImagePath = string
type Quantity = int
type ID = int

type Product = Product of Code * Name * Price * ImagePath

type LineItem = 
  | SaleLineItem of ID * Product * Quantity
  | CancelItem of ID

type Sale = list<LineItem>
type FinalSale = Map<Product, Quantity>

module SaleOperations =
  let CompleteSale (items:Sale) : FinalSale =
    let cancelled =
      items 
      |> Seq.choose (function
        | CancelItem id -> Some id
        | _ -> None)
      |> set
  
    items 
    |> Seq.choose (function
        | SaleLineItem(id, prod, quant) 
            when not (cancelled.Contains(id)) ->
              Some(prod, quant)
        | _ -> None)
    |> Seq.groupBy fst
    |> Seq.map (fun (prod, quants) ->
        prod, quants |> Seq.map snd |> Seq.sum)
    |> Map.ofSeq
  
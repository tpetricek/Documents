namespace Checkout

type Code = string
type Name = string
type Price = decimal
type Product = Product of Code * Name * Price
type Quantity = decimal


// ------------------------------------------------------------------
// Representing purchase as line items

type LineItem = 
  | SaleLineItem of int * Product * Quantity
  | CancelLineItem of int

type LinePurchase = list<LineItem>

// ------------------------------------------------------------------
// Representing aggregated purchase for the bill

type FinalPurchase = Map<Product, Quantity>


// ------------------------------------------------------------------
// Transformation between representations

module Purchase = 

  let processPurchase (line : LinePurchase) : FinalPurchase =
    let cancelled = line |> Seq.choose (function
        | CancelLineItem id -> Some id | _ -> None) |> set
    line 
    |> Seq.choose (function
        | SaleLineItem(id, prod, q) when not(cancelled.Contains(id)) ->
            Some(prod, q)
        | _ -> None)
    |> Seq.groupBy fst
    |> Seq.map (fun (p, pqs) -> 
        p, pqs |> Seq.sumBy snd) 
    |> Map.ofSeq


// ------------------------------------------------------------------
// Sample data

module Products = 
  let products = 
    [ Product("1225995","Tomas Petricek", 25.8M)
      Product("5010459005117","Highland Spring", 0.99M)
      Product("1225993","Phil Trelford", 39.60M) ]

  let Lookup code = 
    products 
    |> Seq.tryFind (function Product(code',_,_) -> code' = code)
// ------------------------------------------------------------------
// Product information

type Code = string
type Name = string
type Price = decimal
type Picture = string
type Quantity = decimal

type Product = Product of Code * Name * Picture * Price

// ------------------------------------------------------------------
// Representing purchase as line items

type LineItem = 
  | SaleLineItem of int * Product * Quantity
  | CancelLineItem of int

type LinePurchase = list<LineItem>

// ------------------------------------------------------------------
// Transformation between representations

let products =
  [ Product("32499021", "Biscuits", "N/A", 1.50M)
    Product("98234923", "Earl Gray Tea", "N/A", 2.60M)
    Product("9770013061206", "The Economist", "N/A", 4.20M) ]

let search code = 
  products |> Seq.find (fun prod ->
    let (Product(id, _, _, _)) = prod
    code = id)

// ------------------------------------------------------------------
// Representing aggregated purchase for the bill

type FinalPurchase = Map<Product, Quantity>

// ------------------------------------------------------------------
// Operation that completes a purchase

let processPurchase (line : LinePurchase) : FinalPurchase =
  // Build a list of cancelled items
  let cancelled = line |> Seq.choose (function
      | CancelLineItem id -> Some id | _ -> None) |> set

  // Pick all items that have not been cancelled
  // Group them by product and add quantities in each group
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
// Testing code interactively

let fin =
  [ SaleLineItem(1, search "9770013061206", 2.0M)
    CancelLineItem(1)
    SaleLineItem(2, search "98234923", 1.0M)
    SaleLineItem(2, search "98234923", 3.0M) ]
  |> processPurchase

fin = Map.ofSeq [ search "98234923", 4.0M ]


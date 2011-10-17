namespace Checkout

// Common types

type Code = string
type Name = string
type Price = decimal
type Product = Product of Code * Name * Price
type Quantity = decimal

/// Tender types
type Tender =
  | Cash
  | Card of string
  | Voucher

/// Represents line item
type LineItem = 
  | SaleLineItem of int * Product * Quantity
  | TenderLineItem of Tender * Price
  | CancelLineItem of int

[<AutoOpen>]
module Products = 
    let products = [
            Product("8717644012208","Lynx Africa",2.49M)
            Product("5010123730215","Listerine",2.49M)
            Product("5000347009242","AquaFresh",1.99M)
            Product("5045094763863","Ibuprofen", 1.99M)]
    let Lookup code = 
        products |> Seq.tryFind (function Product(code',_,_) -> code' = code)

[<AutoOpen>]
module Lines =
    let saleTotal lines =
        lines |> List.sumBy (fun line ->
            match line with
            | SaleLineItem(id,Product(_,_,price),quantity) -> price * quantity
            | _ -> 0.0M
        )
    let tenderTotal lines =
        raise (new System.NotImplementedException())
    let cancelTotal lines =
        raise (new System.NotImplementedException())

type Sale () =
    let mutable items = []    
    member sale.AddItem (item:LineItem) = items <- item::items
    member sale.TotalAmount = saleTotal items - cancelTotal items
    member sale.OutstandingAmount = sale.TotalAmount - tenderTotal items
    member sale.ChangeDue = 
        if sale.OutstandingAmount < 0M 
        then -sale.OutstandingAmount
        else 0M

open NUnit.Framework

[<TestFixture>]
module Tests = 
  open System.Diagnostics

  [<Test>]
  let ``Over tendering with cash should give change`` () =
    let sale = Sale()
    let product = Product("123", "3 Pay As you Go", 10.0M)
    sale.AddItem(SaleLineItem(1, product, 1.0M))
    sale.AddItem(TenderLineItem(Cash,20.00M))
    Assert.That(sale.ChangeDue = 10.0M)

  [<Test>]
  let ``Tendering full amount with card should leave no change`` () =
    let sale = Sale()
    let product = Product("123", "3 Pay As you Go", 10.0M)
    sale.AddItem(SaleLineItem(1, product, 1.0M))
    sale.AddItem(TenderLineItem(Card("1234123412341234"), 10.00M))
    Assert.That(sale.ChangeDue = 0.0M)

  [<Test>]
  let ``Cancelled items should not be charged`` () = 
    let sale = Sale()
    let product = Product("123", "3 Pay As you Go", 10.0M)
    sale.AddItem(SaleLineItem(1, product, 1.0M))
    sale.AddItem(CancelLineItem(1))
    Assert.That(sale.OutstandingAmount = 0.0M)

  do
    ``Over tendering with cash should give change`` ()
    ``Tendering full amount with card should leave no change`` ()
    ``Cancelled items should not be charged`` ()
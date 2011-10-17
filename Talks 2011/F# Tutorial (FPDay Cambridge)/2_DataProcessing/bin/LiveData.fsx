module LiveData
#r "System.Xml.Linq.dll"
open System
open System.IO
open System.Net
open System.Text
open System.Xml.Linq
open FSharp.Control

let private xn (s:string) = XName.op_Implicit(s)
let private (?) (x:XContainer) s = x.Element(xn s)

let getStockPrices ticker = 
  let start (obs:IObserver<_>) =
    async {
      let wc = new WebClient()
      while true do
        do! Async.Sleep(1000)
        let! xml = wc.AsyncDownloadString(Uri("http://services.money.msn.com/quoteservice/streaming?symbol=" + ticker + "&format=xml"))
        let doc = XDocument.Parse(xml)
        let v = float doc?root?result?Dynamic?Last.Value
        obs.OnNext(v) }
    |> Async.StartDisposable
  { new IObservable<_> with
      member x.Subscribe(obs) = start obs }
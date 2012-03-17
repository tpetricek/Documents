module WorldBank.TypeProvider.WorldBank

open System
open System.Net
open System.Web
open System.Xml.Linq

module Internal =
    let wb = "http://www.worldbank.org"

    let xattr s (el:XElement) = el.Attribute(XName.Get(s)).Value
    let xelem s (el:XContainer) = el.Element(XName.Get(s, wb))
    let xelems s (el:XContainer) = el.Elements(XName.Get(s, wb))
    let xvalue (el:XElement) = el.Value
    let xnested path (el:XContainer) = 
        (path |> Seq.fold (fun xn s -> (xelem s xn) :> XContainer) el) :?> XElement
  
    let worldBankUrl functions props = seq { 
        yield "http://api.worldbank.org"
        for item in functions do
            yield "/" + HttpUtility.UrlEncode(item:string)
        yield "?per_page=1000"
        for key, value in props do
            yield "&" + key + "=" + HttpUtility.UrlEncode(value:string) } |> String.concat ""

    let rec worldBankRequest attempt funcs args = async {
        let wc = new WebClient()
        let url = worldBankUrl funcs args
        printfn "[WorldBank] downloading (%d): %s" attempt url
        try
            let! doc = wc.AsyncDownloadString(Uri(url))
            return XDocument.Parse(doc) 
        with e ->
            printfn "[WorldBank] error: %s" (e.ToString()) 
            if attempt > 0 then
                return! worldBankRequest (attempt - 1) funcs args
            else return! failwith "failed" }

    let rec getDocuments funcs args root page = async {
        let! doc = worldBankRequest 5 funcs (args@["page", string page])
        let pages = doc |> xnested [ root ] |> xattr "pages" |> int
        if (pages <= page) then return [doc]
        else 
            let! rest = getDocuments funcs args root (page + 1)
            return doc::rest }

    let getIndicators() = async {
        let! docs = getDocuments ["indicator"] [] "indicators" 1
        return 
            [ for doc in docs do
                for ind in doc |> xelem "indicators" |> xelems "indicator" do
                    yield (ind |> xattr "id", ind |> xelem "name" |> xvalue, ind |> xelem "sourceNote" |> xvalue) ] }

    let getCountries(args) = async {
        let! docs = getDocuments ["country"] args "countries" 1
        return 
            [ for doc in docs do
                for ind in doc |> xelem "countries" |> xelems "country" do
                    yield (ind |> xattr "id", ind |> xelem "name" |> xvalue) ] }

    let getRegions() = async {
        let! docs = getDocuments ["region"] [] "regions" 1
        return 
            [ for doc in docs do
                for ind in doc |> xelem "regions" |> xelems "region" do
                    yield (ind |> xelem "code" |> xvalue, ind |> xelem "name" |> xvalue) ] }

    let getData funcs args key = async {
        let! docs = getDocuments funcs args "data" 1
        return
            [ for doc in docs do
                for ind in doc |> xelem "data" |> xelems "data" do
                    yield (ind |> xelem key |> xvalue, ind |> xelem "value" |> xvalue) ] }

open Internal

let Indicators = lazy (getIndicators() |> Async.RunSynchronously)
let Countries = lazy (getCountries [] |> Async.RunSynchronously)
let Regions = lazy (getRegions() |> Async.RunSynchronously)
  
let GetData funcs args key = getData funcs args key |> Async.RunSynchronously
let GetCountries region = getCountries ["region", region] |> Async.RunSynchronously
  
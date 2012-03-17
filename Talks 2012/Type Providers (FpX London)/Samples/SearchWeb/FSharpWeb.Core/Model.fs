namespace FSharpWeb.Core
open Microsoft.FSharp.Data.TypeProviders

type Netfl = ODataService<"http://odata.netflix.com/Catalog/">
type Tesco = WsdlService<"http://www.techfortesco.com/groceryapi/soapservice.asmx">

type SearchResult =
  { Title : string
    Info : string
    Image : string 
    Link : string
    Source : string }
    
module Model = 

  let searchNetflix (search) =
    let ctx = Netfl.GetDataContext()
    let movies = 
      query { for movie in ctx.Titles do
              where (movie.Name.Contains(search)) 
              take 20
              select movie }
    [ for m in movies ->
        { Title = m.Name; Info = m.Synopsis; Link = m.TinyUrl
          Image = m.BoxArt.MediumUrl; Source = "Netflix" } ]

  let searchTesco (search) = 
    let session = ref null
    let products = ref null
    let totalPages = ref 0
    let totalProducts = ref 0
    
    let service = Tesco.GetSOAPServiceSoap()
    let dkey, appkey = "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA"
    let _ = service.Login("tomas@tomasp.net", "fsharp", dkey, appkey, session)
    let _ = service.ProductSearch(!session, search, false, 1, products, totalPages, totalProducts)

    [ for item in !products ->
        { Title = item.Name; Image = item.ImagePath 
          Info = sprintf "Price: %s (per %s). Maximum items per purchase: %s" 
                         item.PriceDescription item.UnitType item.MaximumPurchaseQuantity
          Link = "http://tesco.com"; Source = "Tesco" } ] 


  let search query =
    Seq.append (searchNetflix query) (searchTesco query)
    |> Seq.sortBy (fun it -> it.Title)
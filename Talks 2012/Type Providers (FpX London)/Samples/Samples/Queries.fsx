#load "Providers.fsx"
open Microsoft.FSharp.Data.TypeProviders

// Query data from Northwind database
type NW = SqlDataConnection<"Data Source=.\\SQLExpress;Initial Catalog=Northwind;Integrated Security=True">
let ctx = NW.GetDataContext()

query { for p in ctx.Products do
        where (p.UnitPrice.Value > 100.0M)
        select p.ProductName }

// Query Netflix movies using OData
type Netflix = ODataService<"http://odata.netflix.com/Catalog/">
let nfl = Netflix.GetDataContext()

let q =
  query { for m in nfl.Titles do
          where (m.Name.Contains("Star"))
          select m }

for movie in q do
  printfn "%s" movie.Name
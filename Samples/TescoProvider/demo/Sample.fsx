#r "../src/bin/Debug/TescoProvider.dll"
#r "System.Runtime.Serialization.dll"

// Connect to Tesco!
let tesco = new Tesco.TescoProvider<"tomas@tomasp.net", "fsharp">()

// Explore categories using on-the-fly generated types
let sherry = tesco.Drinks.Spirits.``Sherry Port & Montilla``








// Query the products in 
// the cateogry and find 
// some fancy port!
query { for s in sherry do
        where (s.Name.Contains("Port"))
        sortByDescending s.UnitPrice
        select (s.Name, s.UnitPrice) }


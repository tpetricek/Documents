#r "packages/FSharp.Data.1.0.13/lib/net40/FSharp.Data.dll"
#r "System.Xml.Linq.dll"

open FSharp.Data

type Demo = XmlProvider<"demo.xml">
let demo = Demo.Load("demo.xml")

demo.Author.Name
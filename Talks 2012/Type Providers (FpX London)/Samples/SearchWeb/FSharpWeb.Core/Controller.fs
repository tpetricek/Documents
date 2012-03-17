namespace FSharpWeb.Core.Controllers

open System
open System.Web.Mvc
open System.Reflection

open FSharpWeb.Core

/// Main controller for ASP.NET MVC pages
[<HandleError>]
type MainController() =
  inherit Controller()

  member x.Index() = 
    x.View()

  member x.Search(id:string) =
    x.ViewData.Model <- Model.search id
    x.View()

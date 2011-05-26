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
    x.ViewData.Model <- Model.load()
    x.View()

  member x.Vote(id:int) =
    Model.vote(id)
    x.RedirectToAction("Index")
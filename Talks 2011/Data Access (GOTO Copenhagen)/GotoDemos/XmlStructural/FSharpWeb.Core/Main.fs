namespace FSharpWeb.Core.Controllers

open System
open System.Web.Mvc
open System.Reflection

open FSharpWeb.Core

/// Main controller for ASP.NET MVC pages
[<HandleError>]
type MainController() =
  inherit Controller()

  member x.Index(id) =
    // Call one of the model's method depending on the ID
    x.ViewData.Model <- 
      if id = "tomasp" then Model.loadTomasP() 
      else Model.loadGuardian()
    x.View()

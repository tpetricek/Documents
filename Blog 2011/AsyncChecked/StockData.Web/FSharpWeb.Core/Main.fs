namespace FSharpWeb.Core.Controllers

open System
open System.Net
open System.Web.Mvc

/// Asynchronous controller that provides a method 
/// for running F# asynchronous workflows
type FSharpAsyncController() = 
  inherit AsyncController()

  member x.RunAsyncAction(workflow) =
    x.AsyncManager.OutstandingOperations.Increment() |> ignore
    async { do! workflow
            x.AsyncManager.OutstandingOperations.Decrement() |> ignore }
    |> Async.Start

/// Main controller for ASP.NET MVC pages
[<HandleError>]
type MainController() =
  inherit FSharpAsyncController()

  member x.Index() = 
    x.View()

  member x.DataAsync(id:string) = 
    async { 
      let url = "http://ichart.finance.yahoo.com/table.csv?s="
      let wc = new WebClient()
      let! data = wc.AsyncDownloadString(Uri(url + id))
      x.ViewData.Model <- data }
    |> x.RunAsyncAction                          

  member x.DataCompleted() =
    x.View()

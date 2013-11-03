namespace FsWeb.Controllers

open System.Web
open System.Web.Mvc
open System.Net.Http
open System.Web.Http
open FsWeb

type DashboardController() =
  inherit ApiController()

  let boxedList list =
    seq [| for key, value in list -> [| box key; box value |] |> box |]

  member x.Get (id:string) = 
    match id with
    | "populationInEuroArea" -> 
        boxedList (WorldData.populationInEuroArea())
    | "populationInOECD" -> 
        boxedList (WorldData.populationInOECD())
    | "populationInUSA" -> 
        boxedList (WorldData.populationInUSA())
    | _ -> 
        boxedList [ "Error - invalid request", 0 ]

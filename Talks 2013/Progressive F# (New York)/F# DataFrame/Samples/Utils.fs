namespace FSharp.DataFrame
open System
open FSharp.DataFrame

[<AutoOpen>]
module ExcelHelper =
  /// Helper operator that takes data from data frame as
  /// 'float' and creates 2D array with them (for Excel)
  let (!!) (frame:Frame<_, _>) =
    frame.Rows |> Series.map (fun k v -> 
        v.As<float>() |> Series.observationsAll |> Seq.map (fun (_, v) -> defaultArg v Double.NaN ))
      |> Series.values |> array2D
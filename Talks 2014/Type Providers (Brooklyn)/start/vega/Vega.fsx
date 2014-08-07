module VegaHub
#I "."
#r "../packages/Deedle.1.0.1/lib/net40/Deedle.dll"
#load "HttpServer.fs"
#load "Grammar.fs"
#load "Basics.fs"

open Deedle
open System
open System.IO
open System.Diagnostics

/// Represents information for vega chart window
/// (to be used by the FSI visualizer)
type VegaChart = internal VegaChart of int * int * string

let private server = ref None
let private id = ref 0

/// Generates 'spec.json' and 'meta.json' in the current folder
/// (spec is a vega specification, meta has margin instructions)
let private show (marginWid, marginHgt) vega = 
  incr id
  let meta = sprintf "{ \"marginWid\":%d, \"marginHgt\":%d, \"id\":%d }" marginWid marginHgt id.Value
  File.WriteAllText(__SOURCE_DIRECTORY__ + "\\spec.json", vega)
  File.WriteAllText(__SOURCE_DIRECTORY__ + "\\meta.json", meta)
  match !server with 
  | None -> server := Some (FSharp.Http.HttpServer.Start("http://localhost:8081/", __SOURCE_DIRECTORY__))
  | _ -> ()

// Register print transformation that displays Vega chart
fsi.AddPrinter(fun (VegaChart(marginWid, marginHgt, vega)) ->
  show (marginWid, marginHgt) vega
  "(Vega)" )

open VegaHub

type Vega private() =
  static member Show(vega) = 
    show (70, 70) vega

  static member Open() =
    Process.Start("http://localhost:8081") |> ignore

  /// Kill the web server
  static member Stop () = 
    match !server with Some s -> s.Stop() | _ -> ()

  static member inline BarColor input =
    let vega =
      Basics.colorBar 
        (List.ofSeq input)
        ( (fun (a, _, _) -> string a),
          (fun (_, b, _) -> float b),
          (fun (_, _, c) -> string c) )
    let maxLabel = 
      input |> Seq.map (fun (_, _, c) -> (string c).Length) |> Seq.max
    let marginWid = 70 + 6 * maxLabel
    VegaChart (marginWid, 50, vega)

  static member inline BarColor(df:Frame<_, _>, y, ?color, ?x) =
    [ for k, row in df.Rows |> Series.observations ->
        try
          let x = match x with None -> string k | Some s -> row.GetAs<string>(s)
          let y = row.GetAs<float>(y)
          let c = match color with None -> "Series" | Some s -> row.GetAs<string>(s)
          [ x, y, c ]
        with :? MissingValueException -> [] ]
    |> List.concat
    |> Vega.BarColor

  static member inline Scatter input =
    let vega =
      Basics.scatterplot 
        (List.ofSeq input)
        ( (fun (a, _, _, _) -> float a),
          (fun (_, b, _, _) -> float b),
          (fun (_, _, c, _) -> string c),
          (fun (_, _, _, d) -> float d) )
    VegaChart (70, 50, vega)

  static member inline Scatter(df:Frame<_, _>, y, ?color, ?size, ?x) =
    [ for k, row in df.Rows |> Series.observations ->
        try
          let x = match x with None -> string k | Some s -> row.GetAs<string>(s)
          let y = row.GetAs<float>(y)
          let c = match color with None -> "Series" | Some s -> row.GetAs<_>(s)
          let s = match color with None -> 20.0 | Some s -> row.GetAs<_>(s)
          [ x, y, c, s ]
        with :? MissingValueException -> [] ]
    |> List.concat
    |> Vega.Scatter

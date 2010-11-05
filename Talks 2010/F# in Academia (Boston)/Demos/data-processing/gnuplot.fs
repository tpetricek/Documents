// ----------------------------------------------------------------------------
// GnuPlot.fs - Provides simple wrappers for calling gnuplot from F#
// (c) Tomas Petricek (tomas@tomasp.net), MS-PL open-source license
// ----------------------------------------------------------------------------
namespace FSharp.GnuPlot

open System
open System.Drawing
open System.Diagnostics

// ----------------------------------------------------------------------------
// Various basic types for representing filling, ranges and for formatting
// ----------------------------------------------------------------------------

/// Represents possible fill styles of a plot
type FillStyle = 
  | Solid
  | Pattern of int


/// Represents an abstract command that sets some property
/// of the plot (and allows undoing the change)
type ICommand = 
  abstract Command : string
  abstract Cleanup : string


/// Utilities for formatting values and for working
/// with commands (this also contains workarounds for Mono bugs)
module InternalUtils = 
  // Some generic functions are not compiled correcty
  // on Mono when using F# Interactive, so we 
  type C<'T> = 
    static member FormatArg(f:'T -> string, a:option<'T>) =
      match a with 
      | None -> ""
      | Some(v) -> (f v)
      
  type D<'T when 'T :> ICommand> = 
    static member CommandList(opt:option<'T>) =
      match opt with 
      | Some(cmd) -> [cmd :> ICommand]
      | None -> []

  /// Formats a value of type option<'a> as a string
  /// using the emtpy string if the value is missing
  let formatArg f a = C<_>.FormatArg(f, a)
  
  /// Formats a value of type option<float>
  let formatNum = formatArg (sprintf "%f")
  
  /// Turns an option value containing some class implementing
  /// ICommand into a list containing exaclty ICommand values
  let commandList opt = D<_>.CommandList(opt)

open InternalUtils

module Internal = 
  /// Type that represents a range for a plot (this type is not
  /// intended to be constructed directly - use 'Range.[ ... ]` instead)
  type Range(?xspec, ?yspec) = 
    let xspec = formatArg (sprintf "set xrange %s\n") xspec
    let yspec = formatArg (sprintf "set yrange %s\n") yspec
    interface ICommand with 
      member x.Command = xspec + yspec
      member x.Cleanup = "set autoscale xy" 

  /// Type that allows elegant construction of ranges specifying both X and Y
  type RangeImplXY() = 
    member x.GetSlice(fx, tx, fy, ty) = 
      Range(sprintf "[%s:%s]" (formatNum fx) (formatNum tx),
            sprintf "[%s:%s]" (formatNum fy) (formatNum ty))

  /// Type that allows elegant construction of ranges specifying only X range
  type RangeImplX() = 
    member x.GetSlice(fx, tx) = 
      Range(xspec = sprintf "[%s:%s]" (formatNum fx) (formatNum tx))

  /// Type that allows elegant construction of ranges specifying only Y range
  type RangeImplY() = 
    member x.GetSlice(fy, ty) = 
      Range(yspec = sprintf "[%s:%s]" (formatNum fy) (formatNum ty))

/// Module with values for elegant construction of ranges
[<AutoOpen>]
module Ranges =
  open Internal
  
  /// Ranges can be constructed using the slicing syntax.
  /// For example 'Range.[-10.0 .. , 2.0 .. 8.0]
  let Range = RangeImplXY()
  /// Ranges can be constructed using the slicing syntax.
  /// For example 'RangeX.[ .. 10.0]
  let RangeX = RangeImplX()
  /// Ranges can be constructed using the slicing syntax.
  /// For example 'RangeY.[ .. 10.0]
  let RangeY = RangeImplY()

// ----------------------------------------------------------------------------
// Formatting of arguments passed to plot and constriction of various types
// of series (Lines, Histogram, ... other options TBD.)
// ----------------------------------------------------------------------------

/// Module that contains formatting of various gnuplot arguments
module InternalFormat =   
  let formatNumArg s = formatArg (sprintf " %s %d" s)
  let formatTitle = formatArg (sprintf " t '%s'")
  let formatColor s = formatArg (fun (color:Color) ->
    sprintf " %s rgb '#%02x%02x%02x'" s color.R color.G color.B)
  let formatFill = formatArg (function
    | Solid -> " fs solid"
    | Pattern(n) -> sprintf " fs pattern %d" n)
  let formatRotate s = formatArg (sprintf "set %s rotate by %d" s)
  let formatTitles tics = formatArg (fun list ->
    let titles = 
      [ for t, n in Seq.zip list [0 .. (List.length list) - 1] do 
          yield sprintf "\"%s\" %d" t n ]
      |> String.concat ", " 
    sprintf "set %s (%s)" tics titles )
     
open InternalFormat

/// Data that are used  as an argument to the 'Series' type
/// For most of the types, we use 'Data', 'Function' is used
/// when specifying function as a string
type Data = 
  | Data of list<float>
  | MultiData of list<float[]>
  | Function of string
  
/// Represents a series of data for the 'plot' function
/// The type can be constructed directly (by setting 'plot' parameter
/// to the desired series type) or indirectly using static
/// members such as 'Series.Histogram'
type Series(plot, data, ?title, ?lineColor, ?weight, ?fill) = 

    /// Helper function for converting data from tuple
  static let indexed4Tuple data =
    data |> List.mapi(fun i (a, b, c, d) -> [| float i; a; b; c; d |])
         |> MultiData
  
  let cmd = 
    (match data with 
     | Data _ -> " '-' using 1 with " + plot
     | MultiData data ->    
         let cols = [ 1 .. (List.head data).Length ] |> List.map string |> String.concat ":"
         sprintf " '-' using %s with %s" cols plot
     | Function f -> f)
      + (formatTitle title) 
      + (formatNumArg "lw" weight)
      + (formatColor "lc" lineColor) 
      + (formatFill fill)
  member x.Data = data
  member x.Command = cmd

  /// Creates a lines data series for a plot  
  static member Lines(data, ?title, ?lineColor, ?weight) = 
    Series("lines", Data data, ?title=title, ?lineColor=lineColor, ?weight=weight)
  /// Creates a finance bars data series for a plot  
  static member FinacneBars(data, ?title, ?lineColor, ?weight) = 
    Series("financebars", indexed4Tuple data, ?title=title, ?lineColor=lineColor, ?weight=weight)
  /// Creates a histogram data series for a plot  
  static member Histogram(data, ?title, ?lineColor, ?weight, ?fill) = 
    Series("histogram", Data data, ?title=title, ?lineColor=lineColor, ?weight=weight, ?fill=fill)
  /// Creates a series specified as a function
  static member Function(func, ?title, ?lineColor, ?weight, ?fill) = 
    Series("", Function func, ?title=title, ?lineColor=lineColor, ?weight=weight, ?fill=fill)


/// Represents a style of a plot (can be passed to the Plot method
/// to set style for single plotting or to the Set method to set the
/// style globally)
type Style(?fill) = 
  let cmd = 
    [ formatFill fill; ]
    |> List.filter (String.IsNullOrEmpty >> not)
    |> List.map (sprintf "set style %s\n")
    |> String.concat ""
  interface ICommand with 
    member x.Command = cmd
    member x.Cleanup = "" // not implemented
  

/// Various outputs that can be specified to gnuplot
type OutputType = 
  | X11
  | Png of string
  | Eps of string

type Output(output:OutputType, ?font) =
  interface ICommand with
    member x.Command = 
      let font = font |> formatArg (sprintf " font '%s'")
      match output with 
      | X11 -> "set term x11" + font
      | Png(s) -> sprintf "set term png%s\nset output '%s'" font s
      | Eps(s) -> sprintf "set term postscript eps enhanced%s\nset output '%s'" font s
    member x.Cleanup = "set term x11"

/// Used to specify titles for the x and y axes. For example:
/// 
///   // specify rotated titles for x axis
///   Titles(x=["one"; "two"], xrotate=-70)
///
type Titles(?x, ?xrotate, ?y, ?yrotate) = 
  let cmd =
    [ formatRotate "xtic" xrotate, "set xtic rotate by 0"
      formatRotate "ytic" yrotate, "set ytic rotate by 0"
      formatTitles "xtics" x, "set xtics auto"
      formatTitles "ytics" y, "set ytics auto" ]
    |> List.filter (fun (s, _) -> s <> "")
  interface ICommand with
    member x.Command = 
      cmd |> List.map fst |> String.concat "\n"
    member x.Cleanup =
      cmd |> List.map snd |> String.concat "\n"
      
// ----------------------------------------------------------------------------
// The main type that wraps the gnuplot process
// ----------------------------------------------------------------------------

/// Provides a wrapper for calling gnuplot from F#. Plots are drawn using
/// the 'Plot' function and can be created using 'Series' type. For example:
///
///   // Plot a function specified as a string
///   gp.Plot("sin(x)")
///
///   // Create a simple line plot
///   gp.Plot(Series.Lines [2.0; 1.0; 2.0; 5.0])   
///
///   // Create a histogram plot drawn using blue color 
///   gp.Plot(Series.Histogram(lineColor=Color.Blue, data=[2.0; 1.0; 2.0; 5.0]))
///
type GnuPlot(?path) =
  // Start the gnuplot process when the class is created
  let path = defaultArg path "gnuplot"
  let gp = 
    new ProcessStartInfo
      (FileName = path, UseShellExecute = false, Arguments = "", 
       RedirectStandardError = true, CreateNoWindow = true, 
       RedirectStandardOutput = true, RedirectStandardInput = true) 
    |> Process.Start

  // Provide event for reading gnuplot messages
  let msgEvent = 
    Event.merge gp.OutputDataReceived gp.ErrorDataReceived
      |> Event.map (fun de -> de.Data)
  do 
    gp.BeginOutputReadLine()  
    gp.BeginErrorReadLine()
    gp.EnableRaisingEvents <- true

  // Send command to gnuplot process
  let sendCommand(str:string) =
    gp.StandardInput.Write(str + "\n")

  // We want to dipose of the running process when the wrapper is disposed
  // The followign bits implement proper 'disposal' pattern
  member private x.Dispose(disposing) = 
   gp.Kill()  
   if disposing then gp.Dispose()

  override x.Finalize() = 
    x.Dispose(false)
    
  interface System.IDisposable with
    member x.Dispose() = 
      x.Dispose(true)
      System.GC.SuppressFinalize(x)

  // Write data to the gnuplot command line
  member private x.WriteData(data:Data) = 
    match data with 
    | MultiData data ->
       for row in data do 
         let row = row |> Array.map string |> String.concat " " 
         x.SendCommand(row)
       x.SendCommand("e")
    | Data data ->
       for row in data do 
         x.SendCommand(string row)
       x.SendCommand("e")
    | Function _ -> ()
    
  // --------------------------------------------------------------------------
  // Public members that can be called by the user
  
  /// Send a string command directly to the gnuplot process
  member x.SendCommand(str) = sendCommand(str)
  
  /// Set a style or a range of the gnuplot session. For example
  ///
  ///   // set fill style to a numbered pattern
  ///   gp.Set(style = Style(fill = Pattern(3)))
  ///  
  ///   // set the X range of the output plot to [-10:]
  ///   gp.Set(range = RangeX.[-10.0 .. ]
  ///
  member x.Set(?style:Style, ?range:Internal.Range, ?output:Output, ?titles:Titles) = 
    let commands = List.concat [ commandList output; commandList style; commandList range; commandList titles ]
    for cmd in commands do
      //printfn "Setting:\n%s" cmd.Command
      x.SendCommand(cmd.Command)

  /// Reset style or range set previously (used mainly internally)
  member x.Unset(?style:Style, ?range:Internal.Range) = 
    let commands = List.concat [ commandList style; commandList range ]
    for cmd in commands do
      if "" <> cmd.Cleanup then x.SendCommand(cmd.Cleanup)
  
  /// Draw a plot specified as a string. Range and style can
  /// be specified using optional parameters. For example:
  ///
  ///  // draw sin(x) function
  ///  gp.Plot("sin(x)")
  ///
  member x.Plot(func:string, ?style, ?range, ?output, ?titles) = 
    x.Plot([Series.Function(func)], ?style=style, ?range=range, ?output=output, ?titles=titles)

  /// Draw a plot of a single data series. Range and style can 
  /// be specified using optional parameters. For example:
  ///
  ///   // Create a simple line plot
  ///   gp.Plot(Series.Lines [2.0; 1.0; 2.0; 5.0],
  ///           range = RangeY.[-1.0 ..])   
  ///    
  member x.Plot(data:Series, ?style, ?range, ?output, ?titles) = 
    x.Plot([data], ?style=style, ?range=range, ?output=output, ?titles=titles)

  /// Draw a plot consisting of multiple data series. Range and 
  /// style can be specified using optional parameters. For example:
  ///
  ///   // Create a simple line plot
  ///   gp.Plot
  ///    [ Series.Lines(title="Lines", data=[2.0; 1.0; 2.0; 5.0])
  ///      Series.Histogram(fill=Solid, data=[2.0; 1.0; 2.0; 5.0]) ]
  ///    
  member x.Plot(data:seq<Series>, ?style:Style, ?range:Internal.Range, ?output:Output, ?titles:Titles) = 
    x.Set(?style=style, ?range=range, ?output=output, ?titles=titles)
    let cmd = 
      "plot \\\n" +
      ( [ for s in data -> s.Command ]
        |> String.concat ", \\\n" )
    //printfn "Command:\n%s" cmd
    x.SendCommand(cmd)
    for s in data do
      x.WriteData(s.Data)
    x.Unset(?style=style, ?range=range)


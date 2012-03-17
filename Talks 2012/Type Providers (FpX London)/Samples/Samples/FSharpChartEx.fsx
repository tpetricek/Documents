#load "FSharpChart.fsx"
namespace MSDN.FSharp.Charting

open System.Drawing
open MSDN.FSharp.Charting
open System.Windows.Forms.DataVisualization.Charting

[<AutoOpen>]
module FSharpChartExtensions =

  type MSDN.FSharp.Charting.FSharpChart with
    /// Draws line chart from year-value pairs using specified label & color
    static member NiceLine(data:seq<int * float>, ?name, ?color) : ChartTypes.GenericChart =
      ( FSharpChart.Line(Array.ofSeq data, Name=defaultArg name "")
        |> FSharpChart.WithSeries.Style(BorderWidth = 2, ?Color = color) )
      :> ChartTypes.GenericChart

    static member NiceChart(chart) =
      let dashGrid = Grid(LineColor = Color.Gainsboro, LineDashStyle = ChartDashStyle.Dash)
      chart
        |> FSharpChart.WithLegend(Docking = Docking.Left)
        |> FSharpChart.WithArea.AxisY(MajorGrid = dashGrid) 
        |> FSharpChart.WithArea.AxisX(MajorGrid = dashGrid)

#nowarn "58"
#load "1 Presidents.fsx"
open ``1 Presidents``
open Deedle
open VegaHub
open RProvider

// ----------------------------------------------------------------------------
// Plotting debt by president 
// ----------------------------------------------------------------------------

// Create professional plots using R's ggplot2 library
open RProvider.ggplot2
open RProvider.``base``

let ed = 
  endDebt |> Frame.dropSparseRows

let labels = 
  ed |> Frame.mapRows (fun y row -> sprintf "(%d) %s" y (row.GetAs("President")))

let qp =
  namedParams [
    "x", box labels.Values
    "y", box ed?Difference.Values
    "fill", box (ed.GetSeries<string>("Party").Values) 
    "geom",  box [| "bar" |] ]
  |> R.qplot

R.assign("q", qp)
R.eval(R.parse(text="q" + 
  """ + scale_fill_manual(values=c("#2080ff", "#ff6040"))""" + 
  """ + theme(axis.text.x = element_text(angle = 90, hjust = 1))"""))

// ----------------------------------------------------------------------------
// Plotting debt by years, colored by party
// ----------------------------------------------------------------------------

let qr = 
  namedParams [
    "x", box aligned.RowKeys
    "y", box aligned?Debt.Values
    "fill", box (aligned.GetSeries<string>("Party").Values)
    "stat", box "identity"
    "geom",  box [| "bar" |] ]
  |> R.qplot

R.assign("q", qr)
R.eval(R.parse(text="q" + 
  """ + scale_fill_manual(values=c("#2080ff", "#ff6040"))""" + 
  """ + theme(axis.text.x = element_text(angle = 90, hjust = 1))"""))

// ----------------------------------------------------------------------------
// Plotting differences by years, colored by party
// ----------------------------------------------------------------------------

// Or we can calculate & plot the differences in debt per year
aligned?Difference <- 
  aligned?Debt |> Series.pairwiseWith (fun _ (v1, v2) -> v2 - v1)

let qd = 
  namedParams [
    "x", box aligned.RowKeys
    "y", box (aligned?Difference |> Series.fillMissingWith 0.0 |> Series.values)
    "fill", box (aligned.GetSeries<string>("Party").Values)
    "stat", box "identity"
    "geom",  box [| "bar" |] ]
  |> R.qplot

R.assign("q", qd)
R.eval(R.parse(text="q" + 
  """ + scale_fill_manual(values=c("#2080ff", "#ff6040"))""" + 
  """ + theme(axis.text.x = element_text(angle = 90, hjust = 1))"""))

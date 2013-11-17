// -------------------------------------------------------
// Classifying price patterns in "live" data
// -------------------------------------------------------

#load "Classifiers.fsx"

open System
open System.Net
open System.Windows
open System.Threading
open Classifiers

let win = new ClassifierWindow(TopMost = true)
win.Run("MSFT")

// -------------------------------------------------------
// Add some classifiers to the window
// -------------------------------------------------------

win.Add("Always up", Price.rising)
win.Add("Mostly up", Price.regression Price.rising)
win.Add("Mostly down", Price.regression Price.declining)

// DEMO 
//   Define mostlyUp, mostlyDown
//   Combine using sequenceAnd to detect V pattern

// DEMO
//   Rising with average price over #30
//   (add average, use map & bothAnd)

// DEMO
//   Calculate difference between lo & hi in the window

// DEMO
//   Getting more fancy with computation expressions

#load "Classifiers.fsx"

open System
open System.Net
open System.Windows
open System.Threading
open Classifiers

// -------------------------------------------------------
// Create classifier window
// -------------------------------------------------------

let win = new ClassifierWindow(TopMost = true)
win.Run("MSFT")

// Add some simple pattern classifiers (always rising never occurs!)
win.Add("Always up", Price.rising)
win.Add("Mostly up", Price.regression Price.rising)
win.Add("Mostly down", Price.regression Price.declining)
win.Add("Average", Price.average)

// Compose pattern for detecting the "V" pattern
let mostlyUp = Price.regression Price.rising
let mostlyDown = Price.regression Price.declining
win.Add("V pattern", Price.sequenceAnd mostlyDown mostlyUp)

// Pattern that detects when price is going up and is above given limit
let highAndRising limit = 
  Price.both
    Price.average
    mostlyUp
  |> Price.map (fun (avg, up) ->
      avg > limit && up)

win.Add("High & rising", highAndRising 30.0)






// ------------------------------------------------------------------
// Simple pattern classifiers

open Classifiers.Price

// Price is always rising (rarely happens)
win.Add("Always rising", rising)

// Price rising over a linear regression
win.Add("Mostly rising", regression rising)
win.Add("Mostly declining", regression declining)


// Classifiers for calculating numeric indicators

// Basic classifiers extract min, max, avg
win.Add("Minimum", minimum)
win.Add("Maximum", maximum)
win.Add("Average", average)

// Calculate difference between min and max
let diff = both minimum maximum |> map (fun (l, h) -> h - l)
win.Add("Difference", diff)

// Detecting interesting patterns 

// Inverse "V" pattern (price goes up, then down)
let upDown = sequenceAnd (regression rising) (regression declining)
win.Add("Up & Down", upDown)

// Classifier checks whether average is less than specified
let averageLessThan lo =
  average |> map (fun v -> v < lo)

// Classifier detects rising price with avg under 26
let risingUnder26 = 
  bothAnd (regression rising) (averageLessThan 26.0)
win.Add("Rising <26", risingUnder26)

// True when difference is greater than specified
let differsBy limit = 
  both minimum maximum |> map (fun (l, h) -> h - l > limit)

// The price is mostly rising and the difference is more than 3
let risingFast = bothAnd (regression rising) (differsBy 3.0)
win.Add("Rising fast", risingFast)


// Computation expression examples

// Price is declining and average is more than 27
let downOver27 = classify { 
  // Calculate average over the range
  let! avg = average
  // Test if the price is mostly declining
  let! down = regression declining
  // Evaluate the condition 
  return down && (avg >= 27.0) }

win.Add("Down >27", downOver27)


// Detecting the "L" patterns & some helpers

// Get the min-max range 
let range = both minimum maximum
// Left side is going down
let leftDown = bothAnd (regression declining) always
win.Add("Left down", leftDown)

// Detect the "L" pattern 
// (Left side goes down & the right side keeps low
// - in range 1/3 from minimum of left side)
let patternL = classify {
  // Get ranges for left & right parts
  let! (lmin, lmax), (rmin, rmax) = sequence range range
  // The left part is declining
  let! decl = leftDown
  
  // The right part keeps in a range
  // (lo +/- of 1/3 difference)
  let offs = (lmax - lmin) / 3.0
  let inRange v = v >= lmin - offs && v <= lmin + offs
  return decl && inRange rmin && inRange rmax } 

win.Add("L pattern", patternL)

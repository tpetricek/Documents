#load "FsLab.fsx"

open FsLab
open System
open Deedle
open RProvider
open FSharp.Data
open FSharp.Charting

// ----------------------------------------------------------------------------
// Exploring Titanic data set
// ----------------------------------------------------------------------------

// Step #0: Load __SOURCE_DIRECTORY__ + "/data/Titanic.csv"
// Step #1: How many people survived the disaster?

// ----------------------------------------------------------------------------
// Working with multilevel indices
// ----------------------------------------------------------------------------

// Step #2: Statistics per passenger class & port of embarkation
//  - Average age for each group
//  - Count the number of passengers
//  - Get the survived column as bools
//  - Aggregate each group (applyLevel) by survival (Seq.countBy)
//    and get nice summary of 'survivals'

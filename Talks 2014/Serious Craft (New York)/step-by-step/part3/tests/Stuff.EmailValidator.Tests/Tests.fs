module FSharp.ProjectScaffold.Tests

open System
open Stuff.EmailValidator
open NUnit.Framework
open FsUnit
open FsCheck
open Recorder

[<Test>]
let ``When password contains a digit, DigitRequirement should be satisfied`` () =
  "unicorn1" |> should equal "unicorn1"
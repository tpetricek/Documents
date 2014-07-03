#if INTERACTIVE
#load "Recorder.fs"
#r "../../bin/Stuff.EmailValidator.dll"
#r "../../packages/FsUnit.1.3.0.1/Lib/Net40/FsUnit.NUnit.dll"
#r "../../packages/NUnit.2.6.3/lib/nunit.framework.dll"
#r "../../packages/FsCheck.0.9.4.0/lib/net40-Client/FsCheck.dll"
#else
module FSharp.ProjectScaffold.Tests
#endif

open System
open Stuff.EmailValidator
open NUnit.Framework
open FsUnit
open FsCheck
open Recorder

[<Test>]
let ``When password contains a digit, DigitRequirement should be satisfied`` () =
  let req = Requirements.DigitRequirement
  let password = "unicorn1"
  let actual = password |> req.IsSatisfied
  // Assert.AreEqual(true, actual)
  actual |> should equal true

[<Test>]
let ``DigitRequirement requires that there is a digit`` () =
  Check.Quick(fun (s:string) -> 
    let req = Requirements.DigitRequirement
    if s <> null then
      let expected = Seq.exists Char.IsDigit s
      req.IsSatisfied(s) |> should equal expected )

[<Test>]
let ``When any requirement is not satisfied, the password should be invalid`` () =
  let password = "invalid"
  let requirement = 
      { new IRequirement with 
          member x.IsSatisfied pwd = false }
  let validator = PowerValidator([requirement])
  validator.IsValid(password) |> should equal false

[<Test>]
let ``Requirement checker should be called with the password`` () =
  let spy = Spy()
  let requirement = 
      { new IRequirement with 
          member x.IsSatisfied pwd = 
            spy.Record pwd
            false }
  let validator = PowerValidator([requirement])
  validator.IsValid("test1") |> ignore
  validator.IsValid("test2") |> ignore
  spy.Calls |> should equal [ "tes1"; "test2" ]
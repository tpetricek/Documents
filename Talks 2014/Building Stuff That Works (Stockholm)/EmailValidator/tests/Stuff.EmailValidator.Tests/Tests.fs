#if INTERACTIVE
#r "../../bin/Stuff.EmailValidator.dll"
#r "../../packages/NUnit.2.6.3/lib/nunit.framework.dll"
#else
module FSharp.ProjectScaffold.Tests
#endif

open Stuff.EmailValidator
open NUnit.Framework

[<Test>]
let ``When password contains a digit, DigitRequirement should be satisfied`` () =
    let req = Requirements.DigitRequirement
    let password = "unicorn1"
    Assert.AreEqual(password |> req.IsSatisfied, true)

[<Test>]
let ``When any requirement is not satisfied, the password should be invalid`` () =
    let password = "invalid"
    let requirement = 
        { new IRequirement with 
            member x.IsSatisfied pwd = false }
    let validator = PowerValidator([requirement])
    Assert.AreEqual(validator.IsValid(password), false)
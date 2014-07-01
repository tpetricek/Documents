(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin"

(**
F# Email Validator
==================

Getting started
---------------

This is how you reference the library:
*)
#r "Stuff.EmailValidator.dll"
open Stuff.EmailValidator
(**

Doing something silly
---------------------

Create basic requirements:
*)
let reqs = 
  [ Requirements.DigitRequirement
    Requirements.LengthRequirement ]
(**
Let's **run this**!!
*)
let p = PowerValidator(reqs)
p.IsValid "Hello1" // fails
p.IsValid "Hello144" // works



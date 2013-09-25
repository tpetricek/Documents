// Given a typical setup (with 'FSharp.Formatting' referenced using NuGet),
// the following will include binaries and load the literate script
#I "lib"
#load "lib/literate.fsx"
open FSharp.Literate

/// This functions processes a single F# Script file
let file = __SOURCE_DIRECTORY__ + "/sample.fsx"
let outputHtml = __SOURCE_DIRECTORY__ + "/sample.html"
let templateHtml = __SOURCE_DIRECTORY__ + "/template.html"
Literate.ProcessScriptFile(file, templateHtml, outputHtml, format = OutputKind.Html)

let outputTex = __SOURCE_DIRECTORY__ + "/sample.tex"
let templateTex = __SOURCE_DIRECTORY__ + "/template.tex"
Literate.ProcessScriptFile(file, templateTex, outputTex, format = OutputKind.Latex)

namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("Stuff.EmailValidator")>]
[<assembly: AssemblyProductAttribute("Stuff.EmailValidator")>]
[<assembly: AssemblyDescriptionAttribute("Stuff that works demo - email validator.")>]
[<assembly: AssemblyVersionAttribute("1.1")>]
[<assembly: AssemblyFileVersionAttribute("1.1")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.1"

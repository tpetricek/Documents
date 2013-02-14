[<ReflectedDefinition>]
module Program

open FunScript

let main() = 
  ()
    


// ------------------------------------------------------------------
// Compile to JavaScript and run simple web server

let components = 
  FunScript.Data.Components.DataProviders @ 
  FunScript.Interop.Components.all
do Runtime.Run(components=components, directory="Web")
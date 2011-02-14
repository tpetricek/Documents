open System
open System.Reflection

// Find all types in all loaded assemblies
let allTypes = 
  [ for a in AppDomain.CurrentDomain.GetAssemblies() do
      for t in a.GetTypes() do
        if t.IsPublic then yield t ]

// Get type name together with member verbosity
// (average length of a member name)
let verbose = 
  [ for t in allTypes do 
      let members = t.GetMembers()
      if members.Length > 0 then
        let length = 
         members 
         |> Seq.averageBy (fun m -> float m.Name.Length)
        yield t.Name, length ]

// Sort using the specified function 'f' and print top 'n'
let report f n = 
  let types = verbose |> List.sortBy (snd >> f) |> Seq.take n 
  let index = ref 1
  for n, v in types do
    printfn "%d - %s (%f)" index.Value n v
    incr index

// Print the 10 least verbose types
report id 10
// Print the 10 most verbose types
report (~-) 10

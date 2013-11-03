#if INTERACTIVE
#r "../../packages/FSharp.Data.1.1.10/lib/net40/FSharp.Data.dll"
#else
module FsWeb.WorldData
#endif
open FSharp.Data

// Display population in EURO area countries
let populationInEuroArea () = [ "TODO", 1 ]

// Display population in top 20 world countries & the rest
let populationInOECD () = [ "TODO", 1 ]

// Get population in different US states
//   Offline: us-states.csv
//   Live: http://visualizing.org/sites/default/files/data_set/admin/NST_EST2009_ALLDATA.csv
let populationInUSA () = [ "TODO", 1 ]
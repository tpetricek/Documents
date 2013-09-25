module Helpers

let memoize f = 
  let dict = System.Collections.Generic.Dictionary<string, decimal>()
  (fun arg ->
      match dict.TryGetValue(arg) with
      | true, res -> res
      | false, _ -> 
          let res = f arg
          dict.[arg] <- res
          res)

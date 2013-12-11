namespace IniFile.TypeProvider

module Seq = 
  let groupAdjacent (f:'T -> 'K option) (input:seq<'T>) = seq {
    let en = input.GetEnumerator()
    let key = ref None
    let values = ref None
    let yieldCurrent () = seq {
      match !key, !values with
      | Some k, Some vs -> yield k, (Array.ofSeq vs) :> seq<_>
      | _ -> () }

    while en.MoveNext() do
      match f en.Current with
      | Some k ->
          yield! yieldCurrent ()
          key := Some k
          values := Some (new ResizeArray<_>())
      | _ when (!values).IsSome ->
          (!values).Value.Add(en.Current) 
      | _ -> ()
    yield! yieldCurrent ()}

module Recorder

type Spy<'T>() =
  let mutable args = []
  member x.Record (arg:'T) = args <- arg::args
  member x.Calls = List.rev args


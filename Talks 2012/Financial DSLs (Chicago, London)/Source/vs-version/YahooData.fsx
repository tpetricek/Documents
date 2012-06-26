#r "bin\\FSharp.AsyncExtensions.dll"

open System
open System.IO
open FSharp.Control

module YahooData = 

  let getPricesWindowed window delay from stock = 

    // Read data as lines from a local file (or from the web)
    let dataLines = asyncSeq {
      let dir = __SOURCE_DIRECTORY__
      let data = 
        File.ReadAllLines(dir + "\\data\\" + stock + ".csv") 
        |> Seq.skip 1 |> Array.ofSeq |> Array.rev
      for line in data do
        yield line }

    // Generates asynchronous sequence with prices
    let prices = asyncSeq {
      // Count values to produce the first window ASAP
      let produced = ref 0
      for line in dataLines do
        // Produce current price
        let infos = (line:string).Split(',')
        let dt = DateTime.Parse(infos.[0])
        let o,h,l,c = float infos.[1], float infos.[2], 
                      float infos.[3], float infos.[4]
        
        if dt > from then
          // Add delay after the initial window
          incr produced
          if !produced > window then do! Async.Sleep(delay)
          yield dt, o }

    prices
    |> AsyncSeq.toObservable
    //|> Observable.windowed window
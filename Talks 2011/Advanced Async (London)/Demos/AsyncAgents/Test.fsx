#load "BlockingQueue.fs"
open AsyncAgents

let ag = new BlockingQueueAgent<int>(3)

let writer() = async { 
    for i in 0 .. 10 do 
        do! ag.AsyncAdd(i)
        printfn "Added: %d" i }

let reader () = async { 
    while true do
        let! v = ag.AsyncTake()
        do! Async.Sleep(1000)
        printfn "Got: %d" v }

reader () |> Async.Start
writer () |> Async.Start

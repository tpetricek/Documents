#load "Utils.fs"
open SocialDrawing

// ----------------------------------------------------------------------------

type Rectangle = 
  { X1 : int; Y1 : int
    X2 : int; Y2 : int
    Color : string }

type Drawing = Rectangle list


type ShapeServerMessage =
  | AddShape of Rectangle
  | GetShapes of AsyncReplyChannel<Drawing>

let agent = Agent.Start(fun inbox -> 
  let rec loop drawing = async {
    let! msg = inbox.Receive()
    match msg with 
    | AddShape(shape) ->
        return! loop (shape::drawing)
    | GetShapes(repl) ->
        repl.Reply(drawing |> List.rev)
        return! loop drawing }
  loop [])

agent.Post(AddShape { X1=100; X2=10; Y1=0; Y2=20; Color="blue" })
agent.PostAndReply(GetShapes)



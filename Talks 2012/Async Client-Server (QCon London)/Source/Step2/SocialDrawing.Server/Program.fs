open System.IO
open System.Net
open SocialDrawing
open System.Web.Script.Serialization

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

// ----------------------------------------------------------------------------

let root = @"C:\QCon\Step2\Web\"

let handleRequest (context:HttpListenerContext) = async { 
  match context.Request.Url.LocalPath with 
  | "/add" -> 
      let serializer = JavaScriptSerializer()
      let json = serializer.DeserializeObject(context.Request.InputString)
      let rect = 
        { X1 = json?X1; Y1 = json?Y1; Color = json?Color
          X2 = json?X2; Y2 = json?Y2; }
      printfn "Adding: %A\n" rect
      agent.Post(AddShape rect)
      context.Response.Reply("OK")

  | "/get" ->
      let serializer = JavaScriptSerializer()
      let! shapes = agent.PostAndAsyncReply(GetShapes)
      printfn "Returning: %d shapes\n" (List.length shapes)
      let json = serializer.Serialize(shapes)
      context.Response.Reply(json)

  | s ->
      try 
        let file = root + (if s = "/" then "index.html" else s)
        let ext = Path.GetExtension(file).ToLower()
        use reader = new StreamReader(file)
        let! text = reader.AsyncReadToEnd()
        context.Response.Reply("text/html", text)
      with e ->
        context.Response.Reply(sprintf "<h2>Error serving: %s</h2>" s) }

// ----------------------------------------------------------------------------

let url = "http://localhost:8081/"
let server = HttpAgent.Start(url, fun mbox -> async {
  while true do 
    let! ctx = mbox.Receive()
    ctx |> handleRequest |> Async.Start })

System.Console.ReadLine() |> ignore
server.Stop()
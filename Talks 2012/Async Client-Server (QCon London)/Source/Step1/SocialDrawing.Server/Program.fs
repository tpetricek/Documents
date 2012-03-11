open System.IO
open System.Net
open SocialDrawing

// ----------------------------------------------------------------------------

let root = @"C:\QCon\Step1\Web\"

let handleRequest (context:HttpListenerContext) = async { 
  match context.Request.Url.LocalPath with 
  | s ->
      try 
        let file = root + (if s = "/" then "index.html" else s)
        let ext = Path.GetExtension(file).ToLower()
        use reader = new StreamReader(file)
        let! text = reader.AsyncReadToEnd()
        context.Response.Reply("text/html", text)
      with e ->
        context.Response.Reply(sprintf "<h2>Error serving: %s</h2>" s) }


let url = "http://localhost:8081/"
let server = HttpAgent.Start(url, fun mbox -> async {
  while true do 
    let! ctx = mbox.Receive()
    ctx |> handleRequest |> Async.Start })


System.Console.ReadLine() |> ignore
server.Stop()
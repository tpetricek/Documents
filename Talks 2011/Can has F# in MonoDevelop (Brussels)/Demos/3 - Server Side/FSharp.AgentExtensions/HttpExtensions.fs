// ----------------------------------------------------------------------------
// Bulking agent
// ----------------------------------------------------------------------------
namespace FSharp.Control

open System.IO
open System.Net
open System.Text
open System.Threading

[<AutoOpen>]
module HttpExtensions = 

  type System.Net.HttpListener with
    member x.AsyncGetContext() = 
      Async.FromBeginEnd(x.BeginGetContext, x.EndGetContext)
    static member Start(url, handler, ?cancellationToken) =
      let server = async { 
        use listener = new HttpListener()
        listener.Prefixes.Add(url)
        listener.Start()
        while true do 
          let! context = listener.AsyncGetContext()
          Async.Start
            ( handler (context.Request, context.Response), 
              ?cancellationToken = cancellationToken) }
      Async.Start(server, ?cancellationToken = cancellationToken)

  type System.Net.HttpListenerRequest with
    member request.InputString =
      use sr = new StreamReader(request.InputStream)
      sr.ReadToEnd()

  type System.Net.HttpListenerResponse with
    member response.Reply(s:string) = 
      let buffer = Encoding.UTF8.GetBytes(s)
      response.ContentLength64 <- int64 buffer.Length
      let output = response.OutputStream
      output.Write(buffer,0,buffer.Length)
      output.Close()
    member response.Reply(typ, buffer:byte[]) = 
      response.ContentLength64 <- int64 buffer.Length
      let output = response.OutputStream
      response.ContentType <- typ
      output.Write(buffer,0,buffer.Length)
      output.Close()

  let contentTypes = 
    dict [ ".gif", "binary/image"
           ".css", "text/css"
           ".html", "text/html" ]


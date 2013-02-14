namespace FSharp.Control

open System
open System.IO
open System.Net
open System.Text
open System.Threading

// ----------------------------------------------------------------------------
// Type alias for agents

type Agent<'T> = MailboxProcessor<'T>

// ----------------------------------------------------------------------------
// Simplify working with request/response

[<AutoOpen>]
module HttpExtensions = 

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
    member response.AsyncReply(s:string) = async {
      let buffer = Encoding.UTF8.GetBytes(s)
      response.ContentLength64 <- int64 buffer.Length
      let output = response.OutputStream
      do! output.AsyncWrite(buffer,0,buffer.Length)
      output.Close() }
    member response.Reply(typ, buffer:byte[]) = async {
      response.ContentLength64 <- int64 buffer.Length
      let output = response.OutputStream
      response.ContentType <- typ
      do! output.AsyncWrite(buffer,0,buffer.Length)
      output.Close() }

  type System.Net.HttpListener with
    member x.AsyncGetContext() = 
      Async.FromBeginEnd(x.BeginGetContext, x.EndGetContext)

// ----------------------------------------------------------------------------
// Utility for writing simple HTTP servers

type HttpServer private (url, f) =
  let tokenSource = new CancellationTokenSource()
  let server = async { 
    use listener = new HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()
    while true do 
      let! context = listener.AsyncGetContext()
      Async.Start(f context, cancellationToken = tokenSource.Token) }
  do Async.Start(server, cancellationToken = tokenSource.Token)
  member x.Stop() = tokenSource.Cancel()
  static member Start(url, f) = 
    new HttpServer(url, f)

// ----------------------------------------------------------------------------
// Extensions for .NET classes that simplify writing HTTP servers (a bit)
// ----------------------------------------------------------------------------
namespace SocialDrawing

[<AutoOpen>]
module HttpExtensions = 

  type System.Net.HttpListener with
    /// Wraps BeginGetContext/EndGetContext method pair and 
    /// exposes the operation using asynchronous workflow
    member x.AsyncGetContext() = 
      Async.FromBeginEnd(x.BeginGetContext, x.EndGetContext)

// ----------------------------------------------------------------------------

open System.Net
open System.Threading

/// Provides nicely named alias for MailboxProcessor
type Agent<'T> = MailboxProcessor<'T>

/// HttpAgent that listens for HTTP requests and handles
/// them using the function provided to the Start method
type HttpAgent private (url, f) as this =
  let tokenSource = new CancellationTokenSource()
  let agent = Agent.Start((fun _ -> f this), tokenSource.Token)
  let server = async { 
    use listener = new HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()
    while true do 
      let! context = listener.AsyncGetContext()
      agent.Post(context) }
  do Async.Start(server, cancellationToken = tokenSource.Token)

  /// Asynchronously waits for the next incomming HTTP request
  /// The method should only be used from the body of the agent
  member x.Receive(?timeout) = agent.Receive(?timeout = timeout)

  /// Stops the HTTP server and releases the TCP connection
  member x.Stop() = tokenSource.Cancel()

  /// Starts new HTTP server on the specified URL. The specified
  /// function represents computation running inside the agent.
  static member Start(url, f) = 
    new HttpAgent(url, f)

// ----------------------------------------------------------------------------

// The HttpListenerContext type has two properties that represent HTTP request 
// and HTTP response respectively. The following listing extends the class 
// representing HTTP request with a member InputString that returns the data 
// sent as part of the request as text. It also extends HTTP response class 
// with an overloaded member Reply that can be used for sending strings or 
// files to the client:

open System.IO
open System.Text

[<AutoOpen>]
module HttpExtensions2 = 
  type System.Net.HttpListenerRequest with
    member request.InputString =
      use sr = new StreamReader(request.InputStream)
      sr.ReadToEnd()

  type System.Net.HttpListenerResponse with
    member response.Reply(s:string) = 
      let buffer = Encoding.UTF8.GetBytes(s)
      response.ContentLength64 <- int64 buffer.Length
      response.OutputStream.Write(buffer,0,buffer.Length)
      response.OutputStream.Close()
    member response.Reply(typ, s:string) = 
      let buffer = Encoding.UTF8.GetBytes(s)
      response.ContentLength64 <- int64 buffer.Length
      response.ContentType <- typ
      response.OutputStream.Write(buffer,0,buffer.Length)
      response.OutputStream.Close()
    member response.Reply(typ, buffer:byte[]) = 
      response.ContentLength64 <- int64 buffer.Length
      response.ContentType <- typ
      response.OutputStream.Write(buffer,0,buffer.Length)
      response.OutputStream.Close()

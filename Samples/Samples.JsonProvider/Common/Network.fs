namespace FSharp.Net

open System
open System.IO
open System.Net
open System.Reflection
open System.Collections.Generic

open FunJS

module Http =

  let formatHeaders headers = 
    [ for h, v in headers -> h + ":" + v ] |> String.concat "\n"

  let parseHeaders (headers:string) = 
    [ for h in headers.Split('\n') do
        if String.IsNullOrEmpty(h) |> not then
          match h.Split(':') with
          | [| h; v |] -> yield h, v
          | _ -> failwithf "Wrong headers: '%s'" headers ]

  /// Hack Uri object using Reflection to make sure that it allows
  /// URL encoded slashes in the Path & query part. Yes, I know.
  /// See: http://stackoverflow.com/questions/781205/getting-a-url-with-an-url-encoded-slash
  let fixUri (uri:Uri) =
    let paq = uri.PathAndQuery
    let flagsFieldInfo = typeof<Uri>.GetField("m_Flags", BindingFlags.Instance ||| BindingFlags.NonPublic)
    let flags = flagsFieldInfo.GetValue(uri) :?> uint64
    let flags = flags &&& (~~~0x30UL)
    flagsFieldInfo.SetValue(uri, flags)
    uri

  /// Downlaod a web resource from the specified URL
  let wget (url:string) = 
    use wc = new WebClient()
    wc.DownloadString(fixUri(Uri(url)))

  /// Downlaod a web resource from the specified URL
  /// (allows specifying query string parameters)
  let asyncWgetEx (url:string) headers query = async {
    let query = 
      [ for (k, v) in query -> k + "=" + v ]
      |> String.concat "&"
    let req = HttpWebRequest.Create(fixUri(Uri(url + "?" + query))) :?> HttpWebRequest

    let keyEquals key (h, v) = 
      let comp = String.Compare(h, key, true, Globalization.CultureInfo.InvariantCulture)
      if comp = 0 then Some v else None

    // Set special headers via proeprty
    let specialHeaders = [ "accept", req.set_Accept ]
    for specialHeader, specialSet in specialHeaders do 
      headers 
      |> Seq.tryPick (keyEquals specialHeader) 
      |> Option.iter (specialSet)
    
    // Set normal headers
    for header, value in headers do
      if specialHeaders |> Seq.forall (keyEquals header >> Option.isNone) then 
        req.Headers.Add(header, value) 

    use! resp = req.AsyncGetResponse()
    use stream = resp.GetResponseStream()
    use sr = new StreamReader(stream)
    return! sr.ReadToEndAsync() |> Async.AwaitTask }

  let wgetEx url headers query = 
    Async.RunSynchronously(asyncWgetEx url headers query)
        
  /// Downlaod a web resource from the specified URL
  /// and create a local cache
  let cachingWget () = 
    let cache = Dictionary<string, string>()
    fun  (url:string) -> 
      match cache.TryGetValue(url) with
      | true, res -> res
      | _ ->
          use wc = new WebClient()
          let res = wc.DownloadString(fixUri(Uri(url)))
          cache.Add(url, res)
          res


namespace FSharp.TwitterAPI

open System
open System.Globalization
open System.Threading
open System.Web
open System.IO
open System.Net
open System.Security.Cryptography
open System.Text
open System.Xml.Linq

/// The results of the parsed tweet
type UserStatus =
    { Id: string
      UserName : string
      ProfileImage : string
      Status : string
      StatusDate : DateTime }

module Utils = 
  let inline xn s = XName.op_Implicit s
  let requestTokenURI = "https://api.twitter.com/oauth/request_token"
  let accessTokenURI = "https://api.twitter.com/oauth/access_token"
  let authorizeURI = "https://api.twitter.com/oauth/authorize"

  /// Attempt to parse a tweet
  let parseTweet (xml: string) =  
      let document = XDocument.Parse xml
      let node = document.Root
      if node.Element(xn "user") <> null then
          let status = node.Element(xn "text").Value |> HttpUtility.HtmlDecode
          Some { Id           = node.Element(xn "user").Element(xn "id").Value
                 UserName     = node.Element(xn "user").Element(xn "screen_name").Value;
                 ProfileImage = node.Element(xn "user").Element(xn "profile_image_url").Value;
                 Status       = status;
                 //Mentions     = [ for userNameCapture in twitterUserName.Matches(status) do yield userNameCapture.Value.[ 1 .. ] ]
                 StatusDate   = node.Element(xn "created_at").Value |> (fun msg ->
                                      DateTime.ParseExact(msg, "ddd MMM dd HH:mm:ss +0000 yyyy",
                                                          CultureInfo.InvariantCulture)); }
      else
          None


  // Utilities
  let unreservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
  let urlEncode str = 
      String.init (String.length str) (fun i -> 
          let symbol = str.[i]
          if unreservedChars.IndexOf(symbol) = -1 then
              "%" + String.Format("{0:X2}", int symbol)
          else
              string symbol)


  // Core Algorithms
  let hmacsha1 signingKey str = 
      let converter = new HMACSHA1(Encoding.ASCII.GetBytes(signingKey : string))
      let inBytes = Encoding.ASCII.GetBytes(str : string)
      let outBytes = converter.ComputeHash(inBytes)
      Convert.ToBase64String(outBytes)

  let compositeSigningKey consumerSecret tokenSecret = 
      urlEncode(consumerSecret) + "&" + urlEncode(tokenSecret)

  let baseString httpMethod baseUri queryParameters = 
      httpMethod + "&" + 
      urlEncode(baseUri) + "&" +
      (queryParameters 
       |> Seq.sortBy (fun (k,v) -> k)
       |> Seq.map (fun (k,v) -> urlEncode(k)+"%3D"+urlEncode(v))
       |> String.concat "%26") 

  let createAuthorizeHeader queryParameters = 
      let headerValue = 
          "OAuth " + 
          (queryParameters
           |> Seq.map (fun (k,v) -> urlEncode(k)+"\x3D\""+urlEncode(v)+"\"")
           |> String.concat ",")
      headerValue

  let currentUnixTime() = floor (DateTime.UtcNow - DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds


  /// Request a token from Twitter and return:
  ///  oauth_token, oauth_token_secret, oauth_callback_confirmed
  let requestToken consumerKey consumerSecret = 
      let signingKey = compositeSigningKey consumerSecret ""

      let queryParameters = 
          ["oauth_callback", "oob";
           "oauth_consumer_key", consumerKey;
           "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
           "oauth_signature_method", "HMAC-SHA1";
           "oauth_timestamp", currentUnixTime().ToString();
           "oauth_version", "1.0"]

      let signingString = baseString "POST" requestTokenURI queryParameters
      let oauth_signature = hmacsha1 signingKey signingString

      let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters

      let req = WebRequest.Create(requestTokenURI, Method="POST")
      let headerValue = createAuthorizeHeader realQueryParameters
      req.Headers.Add(HttpRequestHeader.Authorization, headerValue)
    
      let resp = req.GetResponse()
      let stream = resp.GetResponseStream()
      let txt = (new StreamReader(stream)).ReadToEnd()
    
      let parts = txt.Split('&')
      (parts.[0].Split('=').[1],
       parts.[1].Split('=').[1],
       parts.[2].Split('=').[1] = "true")

  /// Get an access token from Twitter and returns:
  ///   oauth_token, oauth_token_secret
  let accessToken consumerKey consumerSecret token tokenSecret verifier =
      let signingKey = compositeSigningKey consumerSecret tokenSecret

      let queryParameters = 
          ["oauth_consumer_key", consumerKey;
           "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
           "oauth_signature_method", "HMAC-SHA1";
           "oauth_token", token;
           "oauth_timestamp", currentUnixTime().ToString();
           "oauth_verifier", verifier;
           "oauth_version", "1.0"]

      let signingString = baseString "POST" accessTokenURI queryParameters
      let oauth_signature = hmacsha1 signingKey signingString
    
      let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters
    
      let req = WebRequest.Create(accessTokenURI, Method="POST")
      let headerValue = createAuthorizeHeader realQueryParameters
      req.Headers.Add(HttpRequestHeader.Authorization, headerValue)
    
      let resp = req.GetResponse()
      let stream = resp.GetResponseStream()
      let txt = (new StreamReader(stream)).ReadToEnd()
    
      let parts = txt.Split('&')
      (parts.[0].Split('=').[1],
       parts.[1].Split('=').[1])

  /// Compute the 'Authorization' header for the given request data
  let authHeaderAfterAuthenticated consumerKey consumerSecret url httpMethod token tokenSecret queryParams = 
      let signingKey = compositeSigningKey consumerSecret tokenSecret

      let queryParameters = 
              ["oauth_consumer_key", consumerKey;
               "oauth_nonce", System.Guid.NewGuid().ToString().Substring(24);
               "oauth_signature_method", "HMAC-SHA1";
               "oauth_token", token;
               "oauth_timestamp", currentUnixTime().ToString();
               "oauth_version", "1.0"]

      let signingQueryParameters = 
          List.append queryParameters queryParams

      let signingString = baseString httpMethod url signingQueryParameters
      let oauth_signature = hmacsha1 signingKey signingString
      let realQueryParameters = ("oauth_signature", oauth_signature)::queryParameters
      let headerValue = createAuthorizeHeader realQueryParameters
      headerValue

  /// Add an Authorization header to an existing WebRequest 
  let addAuthHeaderForUser consumerKey consumerSecret (webRequest : WebRequest) token tokenSecret queryParams = 
      let url = webRequest.RequestUri.ToString()
      let httpMethod = webRequest.Method
      let header = authHeaderAfterAuthenticated consumerKey consumerSecret url httpMethod token tokenSecret queryParams
      webRequest.Headers.Add(HttpRequestHeader.Authorization, header)

  type System.Net.WebRequest with
      /// Add an Authorization header to the WebRequest for the provided user authorization tokens and query parameters
      member this.AddOAuthHeader(consumerKey, consumerSecret, userToken, userTokenSecret, queryParams) =
          addAuthHeaderForUser consumerKey consumerSecret this userToken userTokenSecret queryParams

open Utils

type TwitterConnection(number:string, key, secret, oauth_token'', oauth_token_secret'') =    
  let oauth_token, oauth_token_secret = accessToken key secret oauth_token'' oauth_token_secret'' number

  let mutable group = new CancellationTokenSource()

  let mutable event = new Event<_>()

  member x.Cancel() = 
    group.Cancel()
    group <- new CancellationTokenSource()
    event <- new Event<_>()

  member x.HomeTimeline() =
    let streamSampleUrl2 = "http://api.twitter.com/1/statuses/home_timeline.xml"
    let req = WebRequest.Create(streamSampleUrl2) 
    req.AddOAuthHeader(key, secret, oauth_token, oauth_token_secret, [])
    let resp = req.GetResponse()
    let strm = resp.GetResponseStream()
    (new StreamReader(strm)).ReadToEnd()

  member x.StartSearch(keywords) =
    let search = keywords |> String.concat ","
    let tweetEvent = new Event<_>()  
    let streamSampleUrl = "http://stream.twitter.com/1/statuses/sample.xml?delimited=length"
    let streamFilterUrl = "http://stream.twitter.com/1/statuses/filter.xml"

    System.Net.ServicePointManager.Expect100Continue <- false
    let listener =
        async { let req = WebRequest.Create("http://stream.twitter.com/1/statuses/filter.xml",  Method = "POST", ContentType = "application/x-www-form-urlencoded")
                req.AddOAuthHeader(key, secret,   oauth_token, oauth_token_secret, ["delimited", "length"; "track", search ])
                do use reqStream = req.GetRequestStream() 
                   use streamWriter = new StreamWriter(reqStream)
                   streamWriter.Write(sprintf "delimited=length&track=%s" search)
                use! resp = 
                    try
                        req.AsyncGetResponse()
                    with 
                    | :? WebException as ex ->
                        let x = ex.Response :?> HttpWebResponse
                        if x.StatusCode = HttpStatusCode.Unauthorized then
                            // TODO need inform user login has failed and they need to try again
                            printfn "Here?? %O" ex
                        reraise()
                use stream = resp.GetResponseStream()
                use reader = new StreamReader(stream)
                let atEnd = reader.EndOfStream
                
                while not reader.EndOfStream do
                  let sizeLine = reader.ReadLine()
                  //printfn "## [%A] read line: %s" DateTime.Now sizeLine
                  if not (String.IsNullOrEmpty sizeLine) then 
                    let size = int sizeLine
                    let buffer = Array.zeroCreate size
                    let _numRead = reader.ReadBlock(buffer,0,size) 
                    //printfn "## [%A] finished reading blockl: %i" DateTime.Now _numRead
                    let text = new System.String(buffer)
                    match parseTweet text with 
                    | Some t -> event.Trigger(t)
                    | _ -> () }
                    //printfn "- %s" text 
 
    Async.Start(listener, group.Token)

  member x.TweetReceived = event.Publish

type TwitterConnector(key, secret, oauth_token'', oauth_token_secret'') =

  static member Authenticate(key, secret, navigate) =
    // Compute URL to send user to to allow our app to connect with their credentials,
    // then open the browser to have them accept
    let oauth_token'', oauth_token_secret'', oauth_callback_confirmed = requestToken key secret
    let url = authorizeURI + "?oauth_token=" + oauth_token''
    navigate(url)
    TwitterConnector(key, secret, oauth_token'', oauth_token_secret'')

  member x.Connect(number) = 
    new TwitterConnection(number, key, secret, oauth_token'', oauth_token_secret'') 

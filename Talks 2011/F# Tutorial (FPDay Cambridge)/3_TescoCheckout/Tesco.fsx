#r "System.Runtime.Serialization.dll"
#r "System.Configuration.dll"
#r "System.ServiceModel.dll"
#I @"TescoCheckout.Api\bin\Debug"
#r "TescoCheckout.Api.dll"

open TescoApi.Tesco
open System.Configuration
open System.ServiceModel
open System.ServiceModel.Configuration

// Dynamically load configuration file from a specified 'app.config'
// (because we don't want to add app.config for fsi.exe to make this work!)
let file = __SOURCE_DIRECTORY__ + "\\TescoApi\\bin\\Debug\\TescoApi.dll.config"
let fileMap = new ExeConfigurationFileMap(ExeConfigFilename = file)
let config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None)
let factory = 
  new ConfigurationChannelFactory<SOAPServiceSoap>
    ("SOAPServiceSoap", config, new EndpointAddress("http://www.techfortesco.com/groceryapi/soapservice.asmx"))

// Login to Tesco
let client = factory.CreateChannel()
let body = LoginRequestBody("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
let login = client.Login(LoginRequest(body))

login.Body.LoginResult.StatusInfo
let session = login.Body.session

type SOAPServiceSoap with
  member x.AsyncLogin(request) = 
    Async.FromBeginEnd(request, x.BeginLogin, x.EndLogin)
  member x.AsyncProductSearch(request) =
    Async.FromBeginEnd(request, x.BeginProductSearch, x.EndProductSearch)

let foo = async {
  let client = factory.CreateChannel()
  let body = LoginRequestBody("tomas@tomasp.net", "fsharp", "fjvRSQvEooAyLq3VhJgJ", "5E7B910E52079C9264CA")
  let login = client.Login(LoginRequest(body))
  let! teas = client.AsyncProductSearch(ProductSearchRequest(ProductSearchRequestBody(session, "tea", true, 1)))
  return () }

// Search for teas...
let teas = client.ProductSearch(ProductSearchRequest(ProductSearchRequestBody(session, "010477323194", true, 1)))
teas.Body.totalPageCount
teas.Body.totalProductCount
for p in teas.Body.products do
  printfn "%s (%s)" p.Name p.ImagePath




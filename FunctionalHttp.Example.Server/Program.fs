namespace FunctionalHttp.Example.Server

open System
open System.IO
open System.Net
open System.Net.Http
open System.IO
open System.Threading
open System.Threading.Tasks

open FunctionalHttp.Core
open FunctionalHttp.Client
open FunctionalHttp.Server

module main =
    let private application = 
        let googleProxyResource =
            let route = Route.Create "/google/*glob"

            let httpClient = 
                (new HttpClient()) 
                |> HttpClient.fromNetHttpClient
                |> HttpClient.usingConverters (Converters.fromUnitToStream, Converters.fromStreamToString)
            
            let get (req:HttpRequest<_>) = async {
                let kvp = Route.getParametersFromUri route req.Uri
                let uri = Uri("http://www.google.com/" + kvp.["glob"])
                let request = HttpRequest.Create(Method.Get, uri, ())
                let! resp = httpClient request
                return 
                    match resp.Entity with
                    | Choice1Of2 entity -> resp |> HttpResponse.withEntity (Some entity)
                    | _ -> resp |> HttpResponse.withEntity None
            }

            let builder = UniformResourceBuilder()
            builder.Route <- route
            builder.Get <- get

            builder.Build()
            |> Resource.authorizing [Authorizer.basic "test" (fun _ -> async.Return true)]
            |> StreamResource.create (fun _ -> Some Converters.fromStreamToString) (fun _ -> Some Converters.fromStringToStream) None
             
        let notFoundResource =
            let handleAndAccept (req:HttpRequest<_>) = 
                HttpResponse.create HttpStatus.clientErrorNotFound None |> async.Return

            (Route.Empty, handleAndAccept, handleAndAccept) 
            |> Resource.create 
            |> StreamResource.create (fun _ -> Some Converters.fromStreamToUnit) (fun _ -> Some Converters.fromUnitToStream) None

        let fileServerResource =
            let route = Route.Create "/files/*path"

            let serialize pref =
                let convert (contentInfo:ContentInfo, fileInfo:FileInfo) =
                    let stream = fileInfo.OpenRead() :> Stream
                    let contentInfo = ContentInfo.Create(length = (uint64 fileInfo.Length))
                    (contentInfo, stream) |> async.Return
                Some convert

            let get (req:HttpRequest<_>) = async {
                let path =
                    let relativePath =
                        let kvp = Route.getParametersFromUri route req.Uri
                        kvp.["path"]
                    let home = Environment.GetEnvironmentVariable("HOME")
                    Path.Combine(home, relativePath)

                let resp =
                    let fileInfo = FileInfo(path)
                    if fileInfo.Exists 
                    then 
                        let fileAttr = File.GetAttributes(path)
                        if fileAttr = FileAttributes.Normal
                        then HttpResponse.create HttpStatus.successOk (Some fileInfo)
                        else HttpResponse.create HttpStatus.clientErrorNotFound None

                    else HttpResponse.create HttpStatus.clientErrorNotFound None
                return resp
            }

            let builder = UniformResourceBuilder()
            builder.Route <- route
            builder.Get <- get

            builder.Build()
            |> StreamResource.create (fun _ -> Some Converters.fromStreamToUnit) serialize None
            |> StreamResource.byteRange

        HttpApplication.routing ([googleProxyResource; fileServerResource], notFoundResource)

    [<EntryPoint>]
    let main argv =         
        let server = HttpServer.create ((fun _ -> application), HttpServer.internalErrorResponseWithStackTrace)

        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let cts = new CancellationTokenSource()

        (listener, cts.Token) |> HttpServer.asListenerConnector server |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
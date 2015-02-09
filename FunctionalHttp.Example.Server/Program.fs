namespace FunctionalHttp.Example.Server

open System
open System.IO
open System.Net
open System.Net.Http
open System.IO
open System.Threading

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
                |> HttpClient.usingConverters (Converters.fromStringToStream, Converters.fromStreamToString)
            
            let get (req:HttpRequest<_>) =
                async {
                    let kvp = Route.getParametersFromUri route req.Uri
                    let uri = Uri("http://www.google.com/" + kvp.["glob"])
                    let request = HttpRequest.Create(Method.Get, uri, "")
                    let! resp = httpClient request
                    return 
                        match resp.Entity with
                        | Choice1Of2 entity -> resp.With(Some(entity))
                        | _ -> resp.With(None)
                }

            let builder = UniformResourceBuilder()
            builder.Route <- route
            builder.Get <- get

            let parse = Converters.fromStreamToString |> HttpRequest.convert
            let serialize (req, resp:HttpResponse<Option<string>>) = 
                match resp.Entity with
                | Some str -> resp.With(str)|> HttpResponse.convertOrThrow Converters.fromStringToStream 
                | None -> resp.With(Stream.Null) |> async.Return

            builder.Build()
            |> Resource.authorizing [Authorizer.basic "test" (fun _ -> async.Return true)]
            |> StreamResource.create (parse, serialize)
             
        let notFoundResource =
            let parse = Converters.fromStreamToString |> HttpRequest.convert

            let serialize (req, resp:HttpResponse<Option<string>>) = 
                match resp.Entity with
                | Some str -> resp.With(str)|> HttpResponse.convertOrThrow Converters.fromStringToStream 
                | None -> resp.With(Stream.Null) |> async.Return

            let handleAndAccept (req:HttpRequest<_>) = 
                HttpResponse<Option<String>>.Create(HttpStatus.clientErrorNotFound, Some (string HttpStatus.clientErrorNotFound)) |> async.Return

            (Route.Empty, handleAndAccept, handleAndAccept) 
            |> Resource.create 
            |> StreamResource.create (parse, serialize)

        HttpApplication.routing ([googleProxyResource], notFoundResource)

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
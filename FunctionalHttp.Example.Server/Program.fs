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
    [<EntryPoint>]
    let main argv =         
        let application = 
            let exampleResource =
                let route = Route.Create "/example/*glob"

                let httpClient = 
                    (new HttpClient()) 
                    |> HttpClient.fromNetHttpClient
                    |> HttpClient.usingConverters (Converters.fromStringToStream, Converters.fromStreamToString)

                let handleAndAccept (req:HttpRequest<_>) = 
                    let kvp = Route.getParametersFromUri route req.Uri

                    Console.WriteLine req

                    let age = TimeSpan(0,0,2)
                    let server = "FunctionalHttp/0.0.1" |> Server.Create
                    let acceptedRanges = Choice1Of2 ([RangeUnit.Bytes] |> Set.ofSeq)
                    let vary = Choice2Of2 Any.Instance
                    let result = HttpResponse<Option<string>>.Create(
                                    HttpStatus.successOk, 
                                    Some (string req),
                                    acceptedRanges = acceptedRanges,
                                    age = age,
                                    allowed = [Method.Get; Method.Put; Method.Post],
                                    lastModified = DateTime.MinValue,
                                    location = Uri("http://www.google.com"),
                                    server = server,
                                    vary = vary)
                    Console.WriteLine result

                    async {
                        let uri = Uri("http://www.google.com/" + kvp.["glob"])
                        let request = HttpRequest.Create(Method.Get, uri, "")
                        let! resp = httpClient request
                        return match resp.Entity with
                                | Choice1Of2 entity -> resp.With(Some(entity))
                                | _ -> resp.With(None)
                    }


                let parse = Converters.fromStreamToString |> HttpRequest.convert
                let serialize (req, resp:HttpResponse<Option<string>>) = 
                    match resp.Entity with
                      | Some str -> resp.With(str)|> HttpResponse.convertOrThrow Converters.fromStringToStream 
                      | None -> resp.With(Stream.Null) |> async.Return

                (route, handleAndAccept, handleAndAccept) 
                |> Resource.create 
                |> Resource.authorizing [Authorizer.basic "test" (fun _ -> async.Return true)]
                |> StreamResource.create (parse, serialize)
                |> StreamResource.byteRange

    
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

            HttpApplication.routing ([exampleResource], notFoundResource)

        let server = HttpServer.create ((fun _ -> application), HttpServer.internalErrorResponseWithStackTrace)

        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let cts = new CancellationTokenSource()

        (listener, cts.Token) |> HttpServer.asListenerConnector server |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
namespace FunctionalHttp.Example.Server

open FunctionalHttp.Core
open FunctionalHttp.Server
open System
open System.IO
open System.Net
open System.Threading

module main =
    [<EntryPoint>]
    let main argv =         
        let application = 
            let route = Route.Create ["example"]

            let handleAndAccept (req:HttpRequest<_>) = 
                Console.WriteLine req
                let server = "FunctionalHttp/0.0.1" |> Server.Create
                let acceptedRanges = Choice1Of2 ([RangeUnit.Bytes] |> Set.ofSeq)
                let result = HttpResponse<Option<string>>.Create(
                                HttpStatus.successOk, 
                                Some (string req),
                                acceptedRanges = acceptedRanges,
                                allowed = [Method.Get; Method.Put; Method.Post],
                                location = Uri("http://www.google.com"),
                                server = server)
                Console.WriteLine result
                result |> async.Return
            let parse = Converters.fromStreamToString |> HttpRequest.convert
            let serialize (req, resp:HttpResponse<Option<string>>) = 
                match resp.Entity with
                  | Some str -> resp.With(str)|> HttpResponse.convertOrThrow Converters.fromStringToStream 
                  | None -> resp.With(Stream.Null) |> async.Return

            (route, handleAndAccept, handleAndAccept) 
            |> Resource.create 
            |> Resource.authorizing [Authorizer.basic "test" (fun _ -> async.Return true)]
            |> StreamResource.create (parse, serialize) |> HttpApplication.singleResource 

        let server = HttpServer.create ((fun _ -> application), HttpServer.internalErrorResponseWithStackTrace)

        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let cts = new CancellationTokenSource()

        (listener, cts.Token) |> HttpServer.asListenerConnector server |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
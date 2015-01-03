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
        let echoResource =
            let route = Route.Create ["example"]

            {new IResource<string, string> with
                member this.Route = route

                member this.Filter (req: HttpRequest<unit>) = req
                member this.Filter (resp: HttpResponse<unit>) = resp

                member this.Handle (req:HttpRequest<unit>) = 
                    HttpResponse<Option<string>>.Create(HttpStatus.successOk, Some (HttpStatus.successOk.ToString()))
                    |> fun x -> async { return x }

                member this.Accept (req: HttpRequest<string>) = 
                    HttpResponse<Option<string>>.Create(HttpStatus.successOk, Some (HttpStatus.successOk.ToString()))
                    |> fun x -> async { return x }

            }

        let application = 
            echoResource 
            |> ServerResource.create (Converters.fromStreamToString |> HttpRequest.convert, 
                                      fun (req, resp) -> 
                                          match resp.Entity with
                                          | Some str -> resp.With(str)|> HttpResponse.convertOrThrow Converters.fromStringToStream 
                                          | None -> resp.With(Stream.Null) |> fun x -> async { return x })  

            |> HttpApplication.singleResource 

        let server = HttpServer.create ((fun _ -> application), HttpServer.internalErrorResponseWithStackTrace)

        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let cts = new CancellationTokenSource()

        (listener, cts.Token) |> HttpServer.asListenerConnector server |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
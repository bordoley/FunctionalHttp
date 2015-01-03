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
                    HttpResponse<Choice<string, exn, unit>>.Create(HttpStatus.successOk, Choice1Of3 (HttpStatus.successOk.ToString()))
                    |> fun x -> async { return x }

                member this.Accept (req: HttpRequest<string>) = 
                    HttpResponse<Choice<string, exn, unit>>.Create(HttpStatus.successOk, Choice1Of3 (HttpStatus.successOk.ToString()))
                    |> fun x -> async { return x }

            }

        let application = 
            echoResource 
            |> ServerResource.create (Converters.fromStreamToString |> HttpRequest.convert, 
                                      fun (req, resp) -> 
                                          match resp.Entity with
                                          | Choice1Of3 str -> resp.With(str)|> HttpResponse.convertOrThrow Converters.fromStringToStream 
                                          | Choice2Of3 exn -> resp.With(Stream.Null) |> fun x -> async { return x }
                                          | Choice3Of3 unit -> resp.With(Stream.Null) |> fun x -> async { return x })  

            |> HttpApplication.singleResource 

        let server = HttpServer.create ((fun _ -> application), HttpServer.internalErrorResponseWithStackTrace)

        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let cts = new CancellationTokenSource()

        (listener, cts.Token) |> HttpServer.asListenerConnector server |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
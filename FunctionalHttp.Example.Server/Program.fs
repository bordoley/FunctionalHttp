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

            {new IResource with
                member this.Route = route
                member this.Filter (req: HttpRequest<unit>) = req
                member this.Filter (resp: HttpResponse<obj>) = resp
                member this.Handle (req:HttpRequest<unit>) = 
                    HttpStatus.successOk
                    |> Status.toResponse
                    |> fun x -> x.With(x.Status.ToString() :> obj)
                    |> fun x -> async { return x }

                member this.Accept (req: HttpRequest<obj>) = 
                    HttpStatus.successOk
                    |> Status.toResponse
                    |> fun x -> x.With(x.Status.ToString() :> obj)
                    |> fun x -> async { return x }
            }

        let application = 
            echoResource 
            |> ServerResource.create (Converters.fromStreamToString |> Converters.compose Converters.fromAnyToObject |> HttpRequest.convert, 
                                      fun (req, resp) -> resp |> (Converters.fromAnyToString |> Converters.compose Converters.fromStringToStream |> HttpResponse.convertOrThrow))              
            |> HttpApplication.singleResource 

        let server = HttpServer.create ((fun _ -> application), HttpServer.internalErrorResponseWithStackTrace)

        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let cts = new CancellationTokenSource()

        (listener, cts.Token) |> HttpServer.asListenerConnector server |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
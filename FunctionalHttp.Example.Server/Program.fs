namespace FunctionalHttp.Example.Server

open FunctionalHttp.Core
open FunctionalHttp.Server
open System
open System.IO
open System.Net
open System.Threading

type EchoResource () =
    let route = Route.Create ["example"]

    interface IServerResource with
        member this.Route = route
        member this.Filter (req: HttpRequest<unit>) = req
        member this.Filter (resp: HttpResponse<obj>) = resp
        member this.Handle (req:HttpRequest<unit>) = 
            HttpStatus.successOk
            |> Status.toResponse
            |> fun x -> x.With(x.Entity :> obj)
            |> fun x -> async { return x }

        member this.Accept (req: HttpRequest<obj>) = 
            HttpStatus.successOk
            |> Status.toResponse
            |> fun x -> x.With(x.Entity :> obj)
            |> fun x -> async { return x }

        member this.Parse (req: HttpRequest<Stream>) = req |> HttpRequest.convert Converters.fromAnyToObject 
        member this.Serialize (req:HttpRequest<_>, resp:HttpResponse<obj>) = resp.With(Stream.Null) |> fun x -> async { return x }

module main =
    [<EntryPoint>]
    let main argv =
        let application = HttpApplication.singleResource (EchoResource() :> IServerResource)
        let server = HttpServer.create ((fun _ -> application), HttpServer.internalErrorResponseWithStackTrace)

        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let cts = new CancellationTokenSource()

        (listener, cts.Token) |> HttpServer.asListenerConnector server |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
namespace FunctionalHttp.Example.Server

open FunctionalHttp.Core
open FunctionalHttp.Server
open System
open System.IO
open System.Net
open System.Threading

type EchoResource () =
    let route = Route.Create ["example"]

    interface IStreamResource with
        member this.Route = route
        member this.Filter (req: HttpRequest<'TFilterReq>) = req
        member this.Filter (resp: HttpResponse<'TFilterResp>) = resp
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
        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let application = HttpApplication.singleResource (EchoResource() :> IStreamResource)

        let server = HttpServer.create ((fun _ -> application), HttpServer.internalErrorResponseWithStackTrace)

        let cts = new CancellationTokenSource()

        HttpListenerServer.create server listener cts.Token |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
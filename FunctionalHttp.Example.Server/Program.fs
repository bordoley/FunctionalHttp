﻿namespace FunctionalHttp.Example.Server

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
        member this.Filter (req: HttpRequest<obj>) = req
        member this.Filter (resp: HttpResponse<obj>) = resp
        member this.Handle (req:HttpRequest<obj>) = HttpStatus.successOk.ToAsyncResponse<obj>()
        member this.Accept (req: HttpRequest<obj>) = HttpStatus.successOk.ToAsyncResponse<obj>()
        member this.Parse (req: HttpRequest<Stream>) = req.WithoutEntityAsync<obj>()
        member this.Serialize (req:HttpRequest<_>, resp:HttpResponse<obj>) = resp.WithoutEntityAsync<Stream>()

module main =
    // FIXME: Should this really even be here. Shouldn't the event loop be part of another libary like AsyncEx, etc.?
    let startOnEventLoop (listener:HttpListener) (applicationProvider:HttpRequest<_> -> IHttpApplication) (cancellationToken:CancellationToken) =
        let eventLoop = EventLoop.current ()
        HttpListenerServer.start listener applicationProvider cancellationToken  |> Async.StartImmediate
        cancellationToken.Register(fun () -> eventLoop.Dispose()) |> ignore
        eventLoop.Run ()

    [<EntryPoint>]
    let main argv =
        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let application = HttpApplication.singleResource (EchoResource() :> IStreamResource)

        let cts = new CancellationTokenSource()

        HttpListenerServer.start listener (fun _ -> application) cts.Token |> Async.StartImmediate

        Console.ReadLine () |> ignore
        cts.Cancel()

        0 // return an integer exit code
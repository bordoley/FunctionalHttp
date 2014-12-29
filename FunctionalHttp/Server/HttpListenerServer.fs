namespace FunctionalHttp.Server

open FunctionalHttp.Core
open FunctionalHttp.Parsing
open System
open System.IO
open System.Linq
open System.Net
open System.Threading
open System.Threading.Tasks

module HttpListenerServer =
    let private parseRequest (req:HttpListenerRequest) =
        let meth = Parser.parse Method.Parser req.HttpMethod |> Option.get
        let version = HttpVersion.Create(req.ProtocolVersion.Major, req.ProtocolVersion.Minor)
        let headers = (req.Headers.AllKeys :> seq<string>) |> Seq.map(fun key -> (key, req.Headers.GetValues(key) :> string seq))

        HttpRequest<Stream>.Create(meth, req.Url, version, Some req.InputStream, headers)

    let private sendResponse (listenerResponse:HttpListenerResponse) (resp:HttpResponse<Stream>) =
        listenerResponse.StatusCode <- resp.Status.Code
        async {
            do!
                match resp.Entity with
                | Some stream -> stream.CopyToAsync(listenerResponse.OutputStream) |> Async.AwaitIAsyncResult |> Async.Ignore
                | _ -> async { return () }
            listenerResponse.Close()
        }

    let start (listener:HttpListener) (applicationProvider:HttpRequest<_> -> IHttpApplication) (cancellationToken:CancellationToken) =
        let listenAndProcessRequest (ctx:HttpListenerContext) =
            async {
                try
                    do! Async.SwitchToThreadPool()
                    let req = parseRequest ctx.Request 
                    let! resp = HttpServer.processRequest applicationProvider req
                    do! sendResponse ctx.Response resp
                with | ex -> ()
            }

        listener.Start ()

        let rec loop () =
            async {
                let! ctx = listener.GetContextAsync() |> Async.AwaitTask
                do! listenAndProcessRequest ctx
                do! loop ()
            }
        loop () |> Async.StartImmediate

    // FIXME: Should this really even be here. Shouldn't the event loop be part of another libary like AsyncEx, etc.?
    let startOnEventLoop (listener:HttpListener) (applicationProvider:HttpRequest<_> -> IHttpApplication) (cancellationToken:CancellationToken) =
        let eventLoop = EventLoop.current ()
        start listener applicationProvider cancellationToken
        cancellationToken.Register(fun () -> eventLoop.Dispose()) |> ignore
        eventLoop.Run ()
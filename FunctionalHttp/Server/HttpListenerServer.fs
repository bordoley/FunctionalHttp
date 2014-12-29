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
        let processRequest (ctx:HttpListenerContext) =
            async {
                try
                    let req = parseRequest ctx.Request 
                    let! resp = HttpServer.processRequest applicationProvider req
                    do! sendResponse ctx.Response resp
                with | ex -> ()
            }

        let rec loop () =
            async {
                cancellationToken.ThrowIfCancellationRequested()
                let! ctx = listener.GetContextAsync() |> Async.AwaitTask
                cancellationToken.ThrowIfCancellationRequested()
                do! processRequest ctx
                do! loop ()
            }

        async {
            if not listener.IsListening then listener.Start ()
            do! loop ()
        }
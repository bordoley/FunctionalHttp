namespace FunctionalHttp.Server

open FunctionalHttp.Collections
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

        HttpRequest<Stream>.Create(meth, req.Url, version,  req.InputStream, headers)

    let private sendResponse (listenerResponse:HttpListenerResponse) (resp:HttpResponse<Stream>) =
        async {
            listenerResponse.StatusCode <- resp.Status.Code
            resp.Location |> Option.map (fun x -> listenerResponse.RedirectLocation <- x.ToString()) |> ignore
            resp.ContentInfo.Length 
                |> Option.map (fun x -> listenerResponse.ContentLength64 <- int64 x) |> ignore
            
            use entity = resp.Entity
            use outStream = listenerResponse.OutputStream
            do! entity.CopyToAsync(outStream) |> Async.AwaitIAsyncResult |> Async.Ignore
        }

    [<CompiledName("Create")>]
    let create (server:IHttpServerDelegate) (listener:HttpListener) (cancellationToken:CancellationToken) =
        let server = HttpServer.processRequest server

        let processRequest (ctx:HttpListenerContext) =
            async {
                try
                    let req = parseRequest ctx.Request 
                    let! resp = server req
                    do! sendResponse ctx.Response resp
                with | ex -> Console.WriteLine ex
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
            listener.Stop()
        }
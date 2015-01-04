﻿namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Collections.Async
open FunctionalHttp.Core
open FunctionalHttp.Parsing
open System.IO
open System.Text 

module HttpServer =
    [<CompiledName("DefaultInternalErrorResponse")>]
    let defaultInternalErrorResponse (exn: exn) = 
        HttpResponse<Stream>.Create(HttpStatus.serverErrorInternalServerError, Stream.Null) |> async.Return

    [<CompiledName("InternalErrorResponseWithStackTrace")>]
    let internalErrorResponseWithStackTrace (exn: exn) =
        let bytes = Encoding.UTF8.GetBytes(exn.ToString().ToCharArray())

        // FIXME: ContentInfo is incomplete here
        // FIXME: Maybe add a serialization package that does this so that it can be reused for
        // both request and response serialization
        let contentInfo = ContentInfo.Create(length = bytes.Length)
        let stream = new MemoryStream(bytes) :> Stream
        HttpResponse<Stream>.Create(HttpStatus.serverErrorInternalServerError, stream, contentInfo = contentInfo) |> async.Return

    [<CompiledName("VirtualHostApplicationProvider")>]
    let virtualHostApplicationProvider (applications:seq<string*IHttpApplication>, defaultApplication:IHttpApplication) =
        let map = Map.ofSeq applications

        fun (req:HttpRequest<unit>) ->
            map.TryFind req.Uri.DnsSafeHost |> Option.getOrElse defaultApplication

    [<CompiledName("Create")>]
    let create (applicationProvider : HttpRequest<Stream> -> IHttpApplication, internalErrorResponse : exn -> Async<HttpResponse<Stream>>) =
        let doProcessRequest (req:HttpRequest<Stream>) = 
            async {
                let app = applicationProvider req
                let req = app.FilterRequest req
                let resource = app.Route req

                return! resource.Handle req |> Async.map app.FilterResponse
            } 

        fun (req:HttpRequest<Stream>) ->
            async {
                let! resp = doProcessRequest req |> Async.Catch
                return! 
                    match resp with
                    | Choice1Of2 resp -> resp |> async.Return
                    | Choice2Of2 exn -> internalErrorResponse exn
            }

#if PCL
#else
    open System
    open System.Net
    open System.Runtime.CompilerServices
    open System.Threading

    [<CompiledName("AsListenerConnector"); Extension>]
    let asListenerConnector (server:HttpRequest<Stream> -> Async<HttpResponse<Stream>>) =
        let parseRequest (req:HttpListenerRequest) =
            let meth = Parser.parse Method.Parser req.HttpMethod |> Option.get
            let version = HttpVersion.Create(req.ProtocolVersion.Major, req.ProtocolVersion.Minor)
            let headers = 
                (req.Headers.AllKeys :> seq<string>) 
                |> Seq.map(fun key -> (key, req.Headers.GetValues(key) :> string seq))
                |> HeaderInternal.headerMapFromRawHeaders

            HttpRequest<Stream>.Create(meth, req.Url, version, headers, req.InputStream)

        let sendResponse (listenerResponse:HttpListenerResponse) (resp:HttpResponse<Stream>) =
            async {
                listenerResponse.StatusCode <- resp.Status.Code
                resp.Location |> Option.map (fun x -> listenerResponse.RedirectLocation <- x.ToString()) |> ignore
                resp.ContentInfo.Length 
                    |> Option.map (fun x -> listenerResponse.ContentLength64 <- int64 x) |> ignore
                
                use entity = resp.Entity
                use outStream = listenerResponse.OutputStream
                do! entity.CopyToAsync(outStream) |> Async.AwaitIAsyncResult |> Async.Ignore
            }

        let processRequest (ctx:HttpListenerContext) =
            async {
                try
                    do! ctx.Request |> parseRequest |> server >>= sendResponse ctx.Response
                with | ex -> Console.WriteLine ex // FIXME: Use a logging framework?
            }

        fun (listener:HttpListener, cancellationToken:CancellationToken) ->
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
#endif
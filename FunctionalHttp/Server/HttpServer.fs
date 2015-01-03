namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open FunctionalHttp.Parsing
open System
open System.IO
open System.Text 
open System.Threading

module HttpServer =
    [<CompiledName("DefaultInternalErrorResponse")>]
    let defaultInternalErrorResponse () = 
        HttpResponse<Stream>.Create(HttpStatus.serverErrorInternalServerError, Stream.Null) |> Async.result

    [<CompiledName("InternalErrorResponseWithStackTrace")>]
    let internalErrorResponseWithStackTrace (exn: exn) =
        let bytes = Encoding.UTF8.GetBytes(exn.ToString().ToCharArray())

        // FIXME: ContentInfo is incomplete here
        // FIXME: Maybe add a serialization package that does this so that it can be reused for
        // both request and response serialization
        let contentInfo = ContentInfo.Create(length = bytes.Length)
        let stream = new MemoryStream(bytes) :> Stream
        HttpResponse<Stream>.Create(HttpStatus.serverErrorInternalServerError, stream, contentInfo = contentInfo) |> Async.result

    [<CompiledName("VirtualHostApplicationProvider")>]
    let virtualHostApplicationProvider (applications:seq<string*IHttpApplication>, defaultApplication:IHttpApplication) =
        let map = Map.ofSeq applications

        fun (req:HttpRequest<unit>) ->
            map.TryFind req.Uri.DnsSafeHost |> Option.getOrElse defaultApplication

    [<CompiledName("Create")>]
    let create (applicationProvider : HttpRequest<Stream> -> IHttpApplication, internalErrorResponse : exn -> Async<HttpResponse<Stream>>) =
        let badRequestResponse =
            HttpResponse<obj>.Create(HttpStatus.clientErrorBadRequest, () :> obj) |> Async.result

        let doProcessRequest (req:HttpRequest<Stream>) = 
            async {
                let app = applicationProvider req
                let req = app.Filter req

                let resource = app.Route req
                let requestStream = req.Entity
                let req = req.With(()) |> resource.Filter

                let! resp = resource.Handle req
                let! resp =
                    if resp.Status <> HttpStatus.informationalContinue
                    then resp |> Async.result
                    else async {
                        let! req = req.With(requestStream) |> resource.Parse 
                        return! 
                            match req.Entity with
                            | Choice1Of2 entity -> resource.Accept (req.With(entity))
                            | Choice2Of2 ex -> badRequestResponse
                    }

                let resp = resource.Filter resp
                let! resp = resource.Serialize (req, resp) 
                return app.Filter resp
            } 

        fun (req:HttpRequest<Stream>) ->
            async {
                let! resp = doProcessRequest req |> Async.Catch
                return! 
                    match resp with
                    | Choice1Of2 resp -> resp |> Async.result
                    | Choice2Of2 exn -> internalErrorResponse exn
            }

#if PCL
#else
    open System.Net
    open System.Runtime.CompilerServices

    [<CompiledName("AsListenerConnector"); Extension>]
    let asListenerConnector (server:HttpRequest<Stream> -> Async<HttpResponse<Stream>>) =
        let parseRequest (req:HttpListenerRequest) =
            let meth = Parser.parse Method.Parser req.HttpMethod |> Option.get
            let version = HttpVersion.Create(req.ProtocolVersion.Major, req.ProtocolVersion.Minor)
            let headers = (req.Headers.AllKeys :> seq<string>) |> Seq.map(fun key -> (key, req.Headers.GetValues(key) :> string seq))

            HttpRequest<Stream>.Create(meth, req.Url, version,  req.InputStream, headers)

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
                    let req = parseRequest ctx.Request 
                    let! resp = server req
                    do! sendResponse ctx.Response resp
                with | ex -> Console.WriteLine ex
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
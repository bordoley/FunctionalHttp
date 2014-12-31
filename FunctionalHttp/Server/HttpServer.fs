namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System.IO
open System.Text

type IHttpServerDelegate =
    abstract member ApplicationProvider : HttpRequest<Stream> -> IHttpApplication
    abstract member InternalErrorResponse : exn -> Async<HttpResponse<Stream>>

module HttpServer =
    [<CompiledName("DefaultInternalErrorResponse")>]
    let defaultInternalErrorResponse = 
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
    let virtualHostApplicationProvider (applications:seq<string*IHttpApplication>) (defaultApplication:IHttpApplication) =
        let map = Map.ofSeq applications

        let provider (req:HttpRequest<_>) = 
            map.TryFind req.Uri.DnsSafeHost |> Option.getOrElse defaultApplication

        provider

    [<CompiledName("Create")>]
    let create (applicationProvider : HttpRequest<Stream> -> IHttpApplication) (internalErrorResponse : exn -> Async<HttpResponse<Stream>>) =
        { 
            new obj()
                interface IHttpServerDelegate with
                    member this.ApplicationProvider req = applicationProvider req
                    member this.InternalErrorResponse exn = internalErrorResponse exn
        }

    [<CompiledName("ProcessRequest")>]
    let processRequest (server:IHttpServerDelegate) (req:HttpRequest<Stream>) =
        let badRequestResponse =
            HttpResponse<obj>.Create(HttpStatus.clientErrorBadRequest, () :> obj) |> Async.result

        let doProcessRequest = async {
            let app = server.ApplicationProvider(req)
            let req2 = app.Filter req
            let resource = app.Route req2
            let req3 = resource.Filter req2
            let! resp = resource.Handle (req3.With(()))
            let! resp2 =
                if resp.Status <> HttpStatus.informationalContinue
                then resp |> Async.result
                else async {
                    let! req4 = resource.Parse req3 |> Async.Catch
                    return! 
                        match req4 with
                        | Choice1Of2 req -> resource.Accept req
                        | Choice2Of2 ex -> badRequestResponse
                }
            let resp3 = resource.Filter resp2
            return! resource.Serialize (req3, resp3)
        } 

        async {
            let! resp = doProcessRequest |> Async.Catch
            return! 
                match resp with
                | Choice1Of2 resp -> resp |> Async.result
                | Choice2Of2 exn -> server.InternalErrorResponse exn
        }
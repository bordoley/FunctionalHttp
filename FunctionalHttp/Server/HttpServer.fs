namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System.IO
open System.Text 

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

        let provider (req:HttpRequest<Stream>) = 
            map.TryFind req.Uri.DnsSafeHost |> Option.getOrElse defaultApplication

        provider

    let internal processRequest (applicationProvider:HttpRequest<Stream> -> IHttpApplication, internalErrorResponse:exn -> Async<HttpResponse<Stream>>) (req:HttpRequest<Stream>) =
        let badRequestResponse =
            HttpResponse<obj>.Create(HttpStatus.clientErrorBadRequest, () :> obj) |> Async.result

        let doProcessRequest = async {
            let app = applicationProvider req
            let req = app.Filter req
            let resource = app.Route req
            let req = resource.Filter req
            let! resp = resource.Handle (req.With(()))
            let! resp =
                if resp.Status <> HttpStatus.informationalContinue
                then resp |> Async.result
                else async {
                    let! req = resource.Parse req
                    return! 
                        match req.Entity with
                        | Choice1Of2 entity -> resource.Accept (req.With(entity))
                        | Choice2Of2 ex -> badRequestResponse
                }
            let resp = resource.Filter resp
            return! resource.Serialize (req, resp)
        } 

        async {
            let! resp = doProcessRequest |> Async.Catch
            return! 
                match resp with
                | Choice1Of2 resp -> resp |> Async.result
                | Choice2Of2 exn -> internalErrorResponse exn
        }

    [<CompiledName("Create")>]
    let create (applicationProvider : HttpRequest<Stream> -> IHttpApplication, internalErrorResponse : exn -> Async<HttpResponse<Stream>>) =
        processRequest (applicationProvider, internalErrorResponse)
namespace FunctionalHttp.Client

open FunctionalHttp.Core

open System
open System.IO
open System.Net
open System.Net.Http

module internal HttpRequest =
    let toHttpRequestMessage (request:HttpRequest<Stream>) =
        let message = new HttpRequestMessage()
        message.RequestUri <- request.Uri
        message.Method <- 
            match request.Method with
            | m when m.Equals(Method.Get) -> HttpMethod.Get
            | m when m.Equals(Method.Post) -> HttpMethod.Post
            | m when m.Equals(Method.Put) -> HttpMethod.Put
            | m when m.Equals(Method.Delete) -> HttpMethod.Delete
            | _ -> failwith "unsupported method"

        request |> HttpRequest.WriteHeaders message.Headers.Add       

        // HTTP Client likes to crash when you set the content on a GET request
        if request.Method <> Method.Get then message.Content <- new StreamContent(request.Entity)

        message

module internal HttpResponse =
    let fromHttpResponseMessageAsync (httpResponseMessage: HttpResponseMessage) =
        async  {
            let statusCode = Status.Create(uint16 httpResponseMessage.StatusCode)
            let version = HttpVersion.Create(uint32  httpResponseMessage.Version.Major, uint32 httpResponseMessage.Version.Minor)

            let headers = 
                Seq.concat [ httpResponseMessage.Headers :> Headers.HttpHeaders ; httpResponseMessage.Content.Headers :> Headers.HttpHeaders]
                |> Seq.map (fun kv -> (kv.Key, kv.Value))
                |> Header.headerMapFromRawHeaders

            let! contentStream = httpResponseMessage.Content.ReadAsStreamAsync() |> Async.AwaitTask

            return HttpResponse<Stream>.Create(statusCode, version, headers, contentStream)
        }

    let fromHttpWebResponse (httpWebResponse : HttpWebResponse) = 
        let status = Status.Create(uint16 httpWebResponse.StatusCode)
        let version = HttpVersion.Create(uint32 httpWebResponse.ProtocolVersion.Major, uint32 httpWebResponse.ProtocolVersion.Minor)

        let headers = 
            httpWebResponse.Headers.AllKeys
            |> Seq.map (fun k -> (k, (httpWebResponse.Headers.GetValues k) :> string seq))
            |> Header.headerMapFromRawHeaders

        HttpResponse<Stream>.Create(status, version, headers, httpWebResponse.GetResponseStream())

    let fromWebException (exn: WebException) =
        match exn.Status with
        | WebExceptionStatus.ProtocolError ->
            let httpWebResponse = exn.Response :?> HttpWebResponse
            fromHttpWebResponse httpWebResponse
        | _ ->
            let status = 
                match exn.Status with
                //| WebExceptionStatus.CacheEntryNotFound -> CacheEntryNotFound
                | WebExceptionStatus.ConnectFailure -> ClientStatus.connectFailure
                | WebExceptionStatus.ConnectionClosed -> ClientStatus.connectionClosed
                | WebExceptionStatus.KeepAliveFailure -> ClientStatus.keepAliveFailure
                | WebExceptionStatus.MessageLengthLimitExceeded -> ClientStatus.messageLengthLimitExceeded
                | WebExceptionStatus.NameResolutionFailure -> ClientStatus.nameResolutionFailure
                //| WebExceptionStatus.Pending -> HttpResponse<'TResp>.Create(Status.SuccessOk)
                | WebExceptionStatus.PipelineFailure -> ClientStatus.pipelineFailure

                | WebExceptionStatus.ProxyNameResolutionFailure -> ClientStatus.proxyNameResolutionFailure
                | WebExceptionStatus.ReceiveFailure -> ClientStatus.receiveFailure
                | WebExceptionStatus.RequestCanceled -> ClientStatus.requestCanceled

                // FIXME: Should these be handled similarly to ProtocolError?
                //| WebExceptionStatus.RequestProhibitedByCachePolicy -> RequestProhibitedByCachePolicy
                //| WebExceptionStatus.RequestProhibitedByProxy -> RequestProhibitedByProxy

                | WebExceptionStatus.SecureChannelFailure -> ClientStatus.secureChannelFailure
                | WebExceptionStatus.SendFailure -> ClientStatus.sendFailure
                | WebExceptionStatus.ServerProtocolViolation -> ClientStatus.serverProtocolViolation

                //| WebExceptionStatus.Success -> Success

                | WebExceptionStatus.Timeout -> ClientStatus.timeout
                | WebExceptionStatus.TrustFailure -> ClientStatus.trustFailure
                | WebExceptionStatus.UnknownError -> ClientStatus.unknownError
                | _ -> Exception("Unknown WebExceptionStatus", exn) |> raise

            let version = Unchecked.defaultof<FunctionalHttp.Core.HttpVersion>
            HttpResponse<Stream>.Create(status, version, Map.empty, Stream.Null)


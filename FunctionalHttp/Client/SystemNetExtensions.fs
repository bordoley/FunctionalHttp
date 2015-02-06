namespace FunctionalHttp.Client

open FunctionalHttp.Core

open System
open System.IO
open System.Net
open System.Net.Http

module internal HttpRequest =
    // See: https://msdn.microsoft.com/en-us/library/system.net.http.headers.httpcontentheaders(v=vs.118).aspx
    let private contentHeaders = 
        [ HttpHeaders.allow; 
          HttpHeaders.contentEncoding;
          HttpHeaders.contentLanguage;
          HttpHeaders.contentLength;
          HttpHeaders.contentLocation;
          HttpHeaders.contentMD5;
          HttpHeaders.contentRange;
          HttpHeaders.contentType;
          HttpHeaders.expires;
          HttpHeaders.lastModified] |> Set.ofList

    let private addHeaders (message:HttpRequestMessage) (header:Header, value:string) =
        if contentHeaders |> Set.contains header 
        then
            if (not (Object.ReferenceEquals (message.Content, null)))
            then message.Content.Headers.Add(header.ToString(), value)
        else message.Headers.Add(string header, value)

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

        // HTTP Client likes to crash when you set the content on a GET request
        if request.Method <> Method.Get then message.Content <- new StreamContent(request.Entity)

        request |> HttpRequest.WriteHeaders (addHeaders message)  

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


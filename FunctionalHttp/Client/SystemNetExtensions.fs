namespace FunctionalHttp.Client

open FunctionalHttp.Collections
open FunctionalHttp.Core
open FunctionalHttp.Client.ClientStatus

open System
open System.IO
open System.Linq
open System.Net
open System.Net.Http
open System.Runtime.CompilerServices

[<AutoOpen>]
module internal HttpWebResponseExtensions =
    type HttpWebResponse with
        member this.ToResponse () =
            let status = Status.Create(uint16 this.StatusCode)
            let version = HttpVersion.Create(uint32 this.ProtocolVersion.Major, uint32 this.ProtocolVersion.Minor)

            let headers = 
                this.Headers.AllKeys
                |> Seq.map (fun k -> (k, (this.Headers.GetValues k) :> string seq))
                |> HeaderInternal.headerMapFromRawHeaders

            HttpResponse<Stream>.Create(status, version, headers, this.GetResponseStream())

[<AutoOpen>]
module internal HttpResponseMessageExtensions =
    type HttpResponseMessage with
        member this.ToAsyncResponse() =
            async  {
                let statusCode = Status.Create(uint16 this.StatusCode)
                let version = HttpVersion.Create(uint32 this.Version.Major, uint32 this.Version.Minor)

                let headers = 
                    Seq.concat [ this.Headers :> Headers.HttpHeaders ; this.Content.Headers :> Headers.HttpHeaders]
                    |> Seq.map (fun kv -> (kv.Key, kv.Value))
                    |> HeaderInternal.headerMapFromRawHeaders

                let! contentStream = this.Content.ReadAsStreamAsync() |> Async.AwaitTask

                return HttpResponse<Stream>.Create(statusCode, version, headers, contentStream)
            }

[<AutoOpen>]
module internal WebExceptionExtensions =
    type WebException with 
        member this.ToStatus() =
            match this.Status with
            //| WebExceptionStatus.CacheEntryNotFound -> CacheEntryNotFound
            | WebExceptionStatus.ConnectFailure -> connectFailure
            | WebExceptionStatus.ConnectionClosed -> connectionClosed
            | WebExceptionStatus.KeepAliveFailure -> keepAliveFailure
            | WebExceptionStatus.MessageLengthLimitExceeded -> messageLengthLimitExceeded
            | WebExceptionStatus.NameResolutionFailure -> nameResolutionFailure
            //| WebExceptionStatus.Pending -> HttpResponse<'TResp>.Create(Status.SuccessOk)
            | WebExceptionStatus.PipelineFailure -> pipelineFailure

            | WebExceptionStatus.ProxyNameResolutionFailure -> proxyNameResolutionFailure
            | WebExceptionStatus.ReceiveFailure -> receiveFailure
            | WebExceptionStatus.RequestCanceled -> requestCanceled

            // FIXME: Should these be handled similarly to ProtocolError?
            //| WebExceptionStatus.RequestProhibitedByCachePolicy -> RequestProhibitedByCachePolicy
            //| WebExceptionStatus.RequestProhibitedByProxy -> RequestProhibitedByProxy

            | WebExceptionStatus.SecureChannelFailure -> secureChannelFailure
            | WebExceptionStatus.SendFailure -> sendFailure
            | WebExceptionStatus.ServerProtocolViolation -> serverProtocolViolation

            //| WebExceptionStatus.Success -> Success

            | WebExceptionStatus.Timeout -> timeout
            | WebExceptionStatus.TrustFailure -> trustFailure
            | WebExceptionStatus.UnknownError -> unknownError
            | _ -> Exception("Unknown WebExceptionStatus", this) |> raise

        member this.ToResponse() =
            match this.Status with
            | WebExceptionStatus.ProtocolError ->
                match this.Response with
                | :? HttpWebResponse as resp -> resp.ToResponse()
                | _ -> Exception("ProtocolError didn't include HttpWebResponse", this) |> raise
            | _ ->  
                let version = Unchecked.defaultof<FunctionalHttp.Core.HttpVersion>
                HttpResponse<Stream>.Create(this.ToStatus(), version, Map.empty, Stream.Null)

[<AbstractClass; Sealed; Extension>]
type internal HttpRequestExtensions private () =
    [<Extension>]
    static member ToHttpRequestMessage(this:HttpRequest<Stream>) =
        let message = new HttpRequestMessage()
        message.RequestUri <- this.Uri
        message.Method <- 
            match this.Method with
            | m when m.Equals(Method.Get) -> HttpMethod.Get
            | m when m.Equals(Method.Post) -> HttpMethod.Post
            | m when m.Equals(Method.Put) -> HttpMethod.Put
            | m when m.Equals(Method.Delete) -> HttpMethod.Delete
            | _ -> failwith "unsupported method"

        match this.Authorization with
        | Some creds -> message.Headers.TryAddWithoutValidation("Authorization", creds.ToString()) |> ignore
        | _ -> ()         

        // HTTP Client likes to crash when you set the content on a GET request
        if this.Method <> Method.Get then message.Content <- new StreamContent(this.Entity)

        message

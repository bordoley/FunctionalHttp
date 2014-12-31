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
            let statusCode = Status.Create(int this.StatusCode)
            let headers =
                this.Headers.AllKeys.SelectMany(fun key -> 
                    this.Headers.GetValues(key).Select(fun value -> (key,value)))

            HttpResponse<Stream>.Create(statusCode, this.GetResponseStream(), headers)

[<AutoOpen>]
module internal HttpResponseMessageExtensions =
    type HttpResponseMessage with
        member this.ToAsyncResponse() =
            async  {
                let statusCode = Status.Create(int this.StatusCode)
                let! contentStream = this.Content.ReadAsStreamAsync() |> Async.AwaitTask
                let headers = this.Headers.SelectMany(fun x -> x.Value.Select(fun v -> (x.Key,v))) 
                let contentHeaders = this.Content.Headers.SelectMany(fun x -> x.Value.Select(fun v -> (x.Key,v)))
                return HttpResponse<Stream>.Create(statusCode,contentStream, headers.Concat(contentHeaders))
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
            | _ -> raise (Exception("Unknown WebExceptionStatus", this))

        member this.ToResponse() =
            match this.Status with
            | WebExceptionStatus.ProtocolError ->
                match this.Response with
                | :? HttpWebResponse as resp -> resp.ToResponse()
                | _ -> raise (Exception("ProtocolError didn't include HttpWebResponse", this))
            | _ ->  this.ToStatus() |> Status.toResponse |> fun x -> x.With(Stream.Null)

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

namespace FunctionalHttp

open FunctionalHttp
open FunctionalHttp.ClientStatus

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

        member this.ToAsyncResponse () = this.ToResponse().ToAsyncResponse()

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
            | WebExceptionStatus.ConnectFailure -> Some ConnectFailure
            | WebExceptionStatus.ConnectionClosed -> Some ConnectionClosed
            | WebExceptionStatus.KeepAliveFailure -> Some KeepAliveFailure
            | WebExceptionStatus.MessageLengthLimitExceeded -> Some MessageLengthLimitExceeded
            | WebExceptionStatus.NameResolutionFailure -> Some NameResolutionFailure
            //| WebExceptionStatus.Pending -> HttpResponse<'TResp>.Create(Status.SuccessOk)
            | WebExceptionStatus.PipelineFailure -> Some PipelineFailure

            | WebExceptionStatus.ProxyNameResolutionFailure -> Some ProxyNameResolutionFailure
            | WebExceptionStatus.ReceiveFailure -> Some ReceiveFailure
            | WebExceptionStatus.RequestCanceled -> Some RequestCanceled

            // FIXME: Should these be handled similarly to ProtocolError?
            //| WebExceptionStatus.RequestProhibitedByCachePolicy -> Some RequestProhibitedByCachePolicy
            //| WebExceptionStatus.RequestProhibitedByProxy -> Some RequestProhibitedByProxy

            | WebExceptionStatus.SecureChannelFailure -> Some SecureChannelFailure
            | WebExceptionStatus.SendFailure -> Some SendFailure
            | WebExceptionStatus.ServerProtocolViolation -> Some ServerProtocolViolation

            //| WebExceptionStatus.Success -> Some Success

            | WebExceptionStatus.Timeout -> Some Timeout
            | WebExceptionStatus.TrustFailure -> Some TrustFailure
            | WebExceptionStatus.UnknownError -> Some UnknownError
            | _ -> None

        member this.ToResponse() =
            match this.Status with
            | WebExceptionStatus.ProtocolError ->
                match this.Response with
                | :? HttpWebResponse as resp -> resp.ToResponse()
                | _ -> raise (new System.Exception("ProtocolError didn't include HttpWebResponse", this))
            | _ -> 
                match this.ToStatus() with
                | Some status -> status.ToResponse<Stream>()
                | None -> raise (new System.Exception("Unknown WebExceptionStatus", this))

        member this.ToAsyncResponse() = this.ToResponse().ToAsyncResponse()

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
        match (this.Method, this.Entity) with
        | (m, Some stream) when m <> Method.Get ->
            message.Content <- new StreamContent(stream)
        | _ -> ()   

        message

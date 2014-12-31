namespace FunctionalHttp.Client

open FunctionalHttp.Core
open System.IO
open System.Text
open System.Threading

type HttpClient<'TReq, 'TResp> = HttpRequest<'TReq> -> Async<HttpResponse<'TResp>>

module HttpClient =
    let usingContext (context:SynchronizationContext) (client:HttpClient<'TReq, 'TResp>) (request:HttpRequest<'TReq>) =
        async {
            do! Async.SwitchToContext context
            return! client request
        }

module ClientStatus =
    [<CompiledName("NetworkUnavailable")>]
    let networkUnavailable = Status.Create(1000, "Network Unavailable")

    // FIXME: These are HttpClient specific error codes.
    // let CacheEntryNotFound = Status.Create(1001, "CacheEntryNotFound")

    [<CompiledName("ConnectFailure")>]
    let connectFailure = Status.Create(1002, "ConnectFailure")

    [<CompiledName("ConnectionClosed")>]
    let connectionClosed = Status.Create(1003, "ConnectionClosed")

    [<CompiledName("KeepAliveFailure")>]
    let keepAliveFailure = Status.Create(1004, "KeepAliveFailure")

    [<CompiledName("MessageLengthLimitExceeded")>]
    let messageLengthLimitExceeded = Status.Create(1005, "MessageLengthLimitExceeded")

    [<CompiledName("NameResolutionFailure")>]
    let nameResolutionFailure = Status.Create(1006, "NameResolutionFailure")

    //let Pending = Status.Create(1007, "Pending")

    [<CompiledName("PipelineFailure")>]
    let pipelineFailure = Status.Create(1008, "PipelineFailure")

    // Included for completeness with WebExceptionStatus. The actual response is converted into the HttpResponse object
    [<CompiledName("ProtocolError")>]
    let protocolError = Status.Create(1009, "ProtocolError")

    [<CompiledName("ProxyNameResolutionFailure")>]
    let proxyNameResolutionFailure = Status.Create(1010, "ProxyNameResolutionFailure")

    [<CompiledName("ReceiveFailure")>]
    let receiveFailure = Status.Create(1011, "ReceiveFailure")

    // FIXME why is this an error, shouldn't it be handled by the whole cancellation token thing?
    [<CompiledName("RequestCanceled")>]
    let requestCanceled = Status.Create(1012, "RequestCanceled")

    // FIXME Should this be an internal exception?
    //let RequestProhibitedByCachePolicy = Status.Create(1013, "RequestProhibitedByCachePolicy")

    // FIXME: Return a standard error for this. Its 400 class
    //let RequestProhibitedByProxy = Status.Create(1014, "RequestProhibitedByProxy")

    [<CompiledName("SecureChannelFailure")>]
    let secureChannelFailure = Status.Create(1015, "SecureChannelFailure")

    [<CompiledName("SendFailure")>]
    let sendFailure = Status.Create(1016, "SendFailure")

    [<CompiledName("ServerProtocolViolation")>]
    let serverProtocolViolation = Status.Create(1017, "ServerProtocolViolation")

    //let Success = Status.Create(1018, "Success")

    [<CompiledName("Timeout")>]
    let timeout = Status.Create(1019, "Timeout")

    [<CompiledName("TrustFailur")>]
    let trustFailure = Status.Create(1020, "TrustFailure")

    [<CompiledName("UnknownError")>]
    let unknownError = Status.Create(1021, "UnknownError")

    [<CompiledName("DeserializationFailed")>]
    let deserializationFailed = Status.Create(1023, "Failed to deserialize response entity")

module HttpResponseDeserializers =
    let toAsyncMemoryStreamResponse (this:HttpResponse<Stream>) =
        async{
            let stream = this.Entity
            let memStream =
                match this.ContentInfo.Length with
                | Some length -> 
                    let byteArray = Array.init<byte> (int length) (fun i -> 0uy)
                    new MemoryStream(byteArray)
                | _ -> new MemoryStream()

            let! copyResult = stream.CopyToAsync(memStream) |> Async.AwaitIAsyncResult |> Async.Catch
            return match copyResult with
                    | Choice1Of2 unit -> this.With(Choice1Of2 memStream)
                    | Choice2Of2 exn -> 
                        HttpResponse<Choice<MemoryStream,exn>>.Create(ClientStatus.deserializationFailed, Choice2Of2 exn, id = this.Id)
        }

    let toAsyncByteArrayResponse (this:HttpResponse<Stream>) =
        async {
            let! memResponse = toAsyncMemoryStreamResponse this
            return
                match memResponse.Entity with
                | Choice1Of2 stream -> memResponse.With(entity = Choice1Of2 (stream.ToArray())) 
                | Choice2Of2 exn -> memResponse.With (Choice2Of2 exn)
        }

    let toAsyncStringResponse (this:HttpResponse<Stream>)  =
        async {
            let encoding = 
                match 
                    this.ContentInfo.MediaType 
                    |> Option.bind (fun mr -> mr.Charset) 
                    |> Option.bind (fun charset -> charset.Encoding)  with
                | Some enc -> enc
                | _ -> Encoding.UTF8

            use sr = new StreamReader(this.Entity, encoding)

            let! result = sr.ReadToEndAsync() |> Async.AwaitTask |> Async.Catch
            return 
                match result with
                | Choice1Of2 result -> this.With(Choice1Of2 result)
                | Choice2Of2 exn ->
                    HttpResponse<Choice<string,exn>>.Create(ClientStatus.deserializationFailed, Choice2Of2 exn, id = this.Id)
        }

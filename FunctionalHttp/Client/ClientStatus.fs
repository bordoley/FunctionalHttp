namespace FunctionalHttp.Client

open FunctionalHttp.Core

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

    [<CompiledName("TrustFailure")>]
    let trustFailure = Status.Create(1020, "TrustFailure")

    [<CompiledName("UnknownError")>]
    let unknownError = Status.Create(1021, "UnknownError")
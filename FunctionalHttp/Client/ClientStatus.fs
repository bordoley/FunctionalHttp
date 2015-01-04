namespace FunctionalHttp.Client

open FunctionalHttp.Core

module ClientStatus =
    [<CompiledName("NetworkUnavailable")>]
    let networkUnavailable = Status.Create(1000)

    // FIXME: These are HttpClient specific error codes.
    // let CacheEntryNotFound = Status.Create(1001, "CacheEntryNotFound")

    [<CompiledName("ConnectFailure")>]
    let connectFailure = Status.Create(1002)

    [<CompiledName("ConnectionClosed")>]
    let connectionClosed = Status.Create(1003)

    [<CompiledName("KeepAliveFailure")>]
    let keepAliveFailure = Status.Create(1004)

    [<CompiledName("MessageLengthLimitExceeded")>]
    let messageLengthLimitExceeded = Status.Create(1005)

    [<CompiledName("NameResolutionFailure")>]
    let nameResolutionFailure = Status.Create(1006)

    //let Pending = Status.Create(1007, "Pending")

    [<CompiledName("PipelineFailure")>]
    let pipelineFailure = Status.Create(1008)

    // Included for completeness with WebExceptionStatus. The actual response is converted into the HttpResponse object
    [<CompiledName("ProtocolError")>]
    let protocolError = Status.Create(1009)

    [<CompiledName("ProxyNameResolutionFailure")>]
    let proxyNameResolutionFailure = Status.Create(1010)

    [<CompiledName("ReceiveFailure")>]
    let receiveFailure = Status.Create(1011)

    // FIXME why is this an error, shouldn't it be handled by the whole cancellation token thing?
    [<CompiledName("RequestCanceled")>]
    let requestCanceled = Status.Create(1012)

    // FIXME Should this be an internal exception?
    //let RequestProhibitedByCachePolicy = Status.Create(1013, "RequestProhibitedByCachePolicy")

    // FIXME: Return a standard error for this. Its 400 class
    //let RequestProhibitedByProxy = Status.Create(1014, "RequestProhibitedByProxy")

    [<CompiledName("SecureChannelFailure")>]
    let secureChannelFailure = Status.Create(1015)

    [<CompiledName("SendFailure")>]
    let sendFailure = Status.Create(1016)

    [<CompiledName("ServerProtocolViolation")>]
    let serverProtocolViolation = Status.Create(1017)

    //let Success = Status.Create(1018, "Success")

    [<CompiledName("Timeout")>]
    let timeout = Status.Create(1019)

    [<CompiledName("TrustFailure")>]
    let trustFailure = Status.Create(1020)

    [<CompiledName("UnknownError")>]
    let unknownError = Status.Create(1021)
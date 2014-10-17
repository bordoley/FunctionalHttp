namespace FunctionalHttp

module ClientStatus =
    let NetworkUnavailable = Status.Create(1000, "Network Unavailable")

    // FIXME: These are HttpClient specific error codes.
    // let CacheEntryNotFound = Status.Create(1001, "CacheEntryNotFound")

    let ConnectFailure = Status.Create(1002, "ConnectFailure")

    let ConnectionClosed = Status.Create(1003, "ConnectionClosed")

    let KeepAliveFailure = Status.Create(1004, "KeepAliveFailure")

    let MessageLengthLimitExceeded = Status.Create(1005, "MessageLengthLimitExceeded")

    let NameResolutionFailure = Status.Create(1006, "NameResolutionFailure")

    //let Pending = Status.Create(1007, "Pending")

    let PipelineFailure = Status.Create(1008, "PipelineFailure")

    // Included for completeness with WebExceptionStatus. The actual response is converted into the HttpResponse object
    let ProtocolError = Status.Create(1009, "ProtocolError")

    let ProxyNameResolutionFailure = Status.Create(1010, "ProxyNameResolutionFailure")

    let ReceiveFailure = Status.Create(1011, "ReceiveFailure")

    // FIXME why is this an error, shouldn't it be handled by the whole cancellation token thing?
    let RequestCanceled = Status.Create(1012, "RequestCanceled")

    // FIXME Should this be an internal exception?
    //let RequestProhibitedByCachePolicy = Status.Create(1013, "RequestProhibitedByCachePolicy")

    // FIXME: Return a standard error for this. Its 400 class
    //let RequestProhibitedByProxy = Status.Create(1014, "RequestProhibitedByProxy")

    let SecureChannelFailure = Status.Create(1015, "SecureChannelFailure")

    let SendFailure = Status.Create(1016, "SendFailure")

    let ServerProtocolViolation = Status.Create(1017, "ServerProtocolViolation")

    //let Success = Status.Create(1018, "Success")

    let Timeout = Status.Create(1019, "Timeout")

    let TrustFailure = Status.Create(1020, "TrustFailure")

    let UnknownError = Status.Create(1021, "UnknownError")

    let DeserializationFailed = Status.Create(1023, "Failed to deserialize response entity")

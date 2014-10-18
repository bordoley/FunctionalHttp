namespace FunctionalHttp

type Status = 
    private {
        code:int
        msg:string
    }

    static member ClientErrorBadRequest = { code = 400; msg = "Bad Request"}

    static member ClientErrorConflict = { code = 409; msg = "Conflict"}

    static member ClientErrorExpectaionFailed = { code = 417; msg = "Expectation Failed"}

    static member ClientErrorFailedDependency = { code = 424; msg = "Failed Dependency"}

    static member ClientErrorForbidden = { code = 403; msg = "Forbidden"}

    static member ClientErrorGone = { code = 410; msg = "Gone"}

    static member ClientErrorLengthRequired = { code = 411; msg = "Length Required"}

    static member ClientErrorLocked = { code = 423; msg = "Locked"}

    static member ClientErrorMethodNotAllowed = { code = 405; msg = "Method Not Allowed"}

    static member ClientErrorNotAcceptable = { code = 406; msg = "Not Acceptable"}

    static member ClientErrorNotFound = { code = 404; msg = "Not Found"}

    static member ClientErrorPreconditionFailed = { code = 412; msg = "Precondition Failed"}

    static member ClientErrorProxyAuthenticated = { code = 407; msg = "Proxy Authentication Required"}

    static member ClientErrorRequestEntityTooLarge = { code = 413; msg = "Request Entity Too Large"}

    static member ClientErrorRequestTimeout = { code = 408; msg = "Request Timeout"}

    static member ClientErrorRequestUriTooLong = { code = 414; msg = "Request-URI Too Long"}

    static member ClientErrorRequestedRangeNotSatisfiable = { code = 416; msg = "Requested Range Not Satisfiable"}

    static member ClientErrorUnauthorized = { code = 401; msg = "Unauthorized"}

    static member ClientErrorUnprocessableEntity = { code = 422; msg = "Unprocessable Entity"}

    static member ClientErrorUnsupportedMediaType = { code = 415; msg = "Unsupported Media Type"}

    static member ClientErrorUpgradeRequired = { code = 426; msg = "Upgrade Required"}

    static member InformationalContinue = { code = 100; msg = "Continue"}

    static member InformationalProcessing = { code = 102; msg = "Processing"}
    
    static member InformationalSwitchingProtocols = { code = 101; msg = "Switching Protocols"}

    static member RedirectionFound = { code = 302; msg = "Found"}

    static member RedirectionMovedPermanently = { code = 301; msg = "Moved Permanently"}

    static member RedirectionMultipleChoices = { code = 300; msg = "Multiple Choices"}

    static member RedirectionNotModified = { code = 304; msg = "Not Modified"}

    static member RedirectionSeeOther = { code = 303; msg = "See Other"}

    static member RedirectionTemporaryRedirect = { code = 307; msg = "Temporary Redirect"}

    static member RedirectionUseProxy = { code = 305; msg = "Use Proxy"}

    static member ServerErrorBadGateway = { code = 502; msg = "Bad Gateway"}

    static member ServerErrorGatewayTimeout = { code = 504; msg = "Gateway Timeout"}

    static member ServerErrorHttpVersionNotSupported = { code = 505; msg = "HTTP Version Not Supported"}

    static member ServerErrorInsufficientStorage = { code = 507; msg = "Insufficient Storage"}

    static member ServerErrorInternalServerError = { code = 500; msg = "Internal Server Error"}

    static member ServerErrorLoopDetected = { code = 508; msg = "Loop Detected"}

    static member ServerErrorNotExtended = { code = 510; msg = "Not Extended"}

    static member ServerErrorNotImplemented = { code = 501; msg = "Not Implemented"}

    static member ServerErrorServiceUnavailable = { code = 503; msg = "Service Unavailable"}

    static member ServerErrorVariantAlsoNegotiates = { code = 506; msg = "Variant Also Negotiates"}

    static member SuccessAccepted = { code = 202; msg = "Accepted"}

    static member SuccessAlreadyReported = { code = 208; msg = "Already Reported"}

    static member SuccessCreated = { code = 201; msg = "Created"}

    static member SuccessImUsed = { code = 226; msg = "IM Used"}

    static member SuccessMultiStatus = { code = 207; msg = "Multi-Status"}

    static member SuccessNoContent = { code = 204; msg = "No Content"}

    static member SuccessNonAuthoritativeInformation = { code = 203; msg = "Non-Authoritative Information"}

    static member SuccessOk = { code = 200; msg = "OK"}

    static member SuccessPartialContent = { code = 206; msg = "Partial Content"}

    static member SuccessResetContent = { code = 205; msg = "Reset Content"}

    static member Create(code) = { code = code; msg = "" }

    static member Create(code, msg) = { code = code; msg = msg } 

    member this.Code = this.code
    
    member this.Message = this.msg

    override this.ToString() = this.Message

type StatusClass = 
    | ClientError | Informational | Redirection | ServerError | Success | SystemHttpClientError
    static member From (status:Status) =
        match status.Code with  
        | x when x >= 100 && x < 200 -> StatusClass.Informational 
        | x when x >= 200 && x < 300 -> StatusClass.Success 
        | x when x >= 300 && x < 400 -> StatusClass.Redirection 
        | x when x >= 400 && x < 500 -> StatusClass.ClientError
        | x when x >= 500 && x < 600 -> StatusClass.ServerError 
        | x when x >= 1000 && x < 2000 -> StatusClass.SystemHttpClientError 
        | _ -> failwith "Invalid status code"  


[<AutoOpen>]  
module StatusMixins =
    type Status with 
        member this.Class 
            with get() = StatusClass.From(this)

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

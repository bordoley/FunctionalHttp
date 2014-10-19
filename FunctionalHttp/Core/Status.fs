namespace FunctionalHttp

type Status = 
    private {
        code:int
        msg:string
    }

    static member internal ClientErrorBadRequest = { code = 400; msg = "Bad Request"}

    static member internal ClientErrorConflict = { code = 409; msg = "Conflict"}

    static member internal ClientErrorExpectationFailed = { code = 417; msg = "Expectation Failed"}

    static member internal ClientErrorFailedDependency = { code = 424; msg = "Failed Dependency"}

    static member internal ClientErrorForbidden = { code = 403; msg = "Forbidden"}

    static member internal ClientErrorGone = { code = 410; msg = "Gone"}

    static member internal ClientErrorLengthRequired = { code = 411; msg = "Length Required"}

    static member internal ClientErrorLocked = { code = 423; msg = "Locked"}

    static member internal ClientErrorMethodNotAllowed = { code = 405; msg = "Method Not Allowed"}

    static member internal ClientErrorNotAcceptable = { code = 406; msg = "Not Acceptable"}

    static member internal ClientErrorNotFound = { code = 404; msg = "Not Found"}

    static member internal ClientErrorPreconditionFailed = { code = 412; msg = "Precondition Failed"}

    static member internal ClientErrorProxyAuthenticated = { code = 407; msg = "Proxy Authentication Required"}

    static member internal ClientErrorRequestEntityTooLarge = { code = 413; msg = "Request Entity Too Large"}

    static member internal ClientErrorRequestTimeout = { code = 408; msg = "Request Timeout"}

    static member internal ClientErrorRequestUriTooLong = { code = 414; msg = "Request-URI Too Long"}

    static member internal ClientErrorRequestedRangeNotSatisfiable = { code = 416; msg = "Requested Range Not Satisfiable"}

    static member internal ClientErrorUnauthorized = { code = 401; msg = "Unauthorized"}

    static member internal ClientErrorUnprocessableEntity = { code = 422; msg = "Unprocessable Entity"}

    static member internal ClientErrorUnsupportedMediaType = { code = 415; msg = "Unsupported Media Type"}

    static member internal ClientErrorUpgradeRequired = { code = 426; msg = "Upgrade Required"}

    static member internal InformationalContinue = { code = 100; msg = "Continue"}

    static member internal InformationalProcessing = { code = 102; msg = "Processing"}
    
    static member internal InformationalSwitchingProtocols = { code = 101; msg = "Switching Protocols"}

    static member internal RedirectionFound = { code = 302; msg = "Found"}

    static member internal RedirectionMovedPermanently = { code = 301; msg = "Moved Permanently"}

    static member internal RedirectionMultipleChoices = { code = 300; msg = "Multiple Choices"}

    static member internal RedirectionNotModified = { code = 304; msg = "Not Modified"}

    static member internal RedirectionSeeOther = { code = 303; msg = "See Other"}

    static member internal RedirectionTemporaryRedirect = { code = 307; msg = "Temporary Redirect"}

    static member internal RedirectionUseProxy = { code = 305; msg = "Use Proxy"}

    static member internal ServerErrorBadGateway = { code = 502; msg = "Bad Gateway"}

    static member internal ServerErrorGatewayTimeout = { code = 504; msg = "Gateway Timeout"}

    static member internal ServerErrorHttpVersionNotSupported = { code = 505; msg = "HTTP Version Not Supported"}

    static member internal ServerErrorInsufficientStorage = { code = 507; msg = "Insufficient Storage"}

    static member internal ServerErrorInternalServerError = { code = 500; msg = "Internal Server Error"}

    static member internal ServerErrorLoopDetected = { code = 508; msg = "Loop Detected"}

    static member internal ServerErrorNotExtended = { code = 510; msg = "Not Extended"}

    static member internal ServerErrorNotImplemented = { code = 501; msg = "Not Implemented"}

    static member internal ServerErrorServiceUnavailable = { code = 503; msg = "Service Unavailable"}

    static member internal ServerErrorVariantAlsoNegotiates = { code = 506; msg = "Variant Also Negotiates"}

    static member internal SuccessAccepted = { code = 202; msg = "Accepted"}

    static member internal SuccessAlreadyReported = { code = 208; msg = "Already Reported"}

    static member internal SuccessCreated = { code = 201; msg = "Created"}

    static member internal SuccessImUsed = { code = 226; msg = "IM Used"}

    static member internal SuccessMultiStatus = { code = 207; msg = "Multi-Status"}

    static member internal SuccessNoContent = { code = 204; msg = "No Content"}

    static member internal SuccessNonAuthoritativeInformation = { code = 203; msg = "Non-Authoritative Information"}

    static member internal SuccessOk = { code = 200; msg = "OK"}

    static member internal SuccessPartialContent = { code = 206; msg = "Partial Content"}

    static member internal SuccessResetContent = { code = 205; msg = "Reset Content"}

    static member Create(code) = Status.Create(code, "")

    static member Create(code, msg) = 
        match code with
        | 100 -> Status.InformationalContinue
        | 101 -> Status.InformationalSwitchingProtocols
        | 102 -> Status.InformationalProcessing
        | 200 -> Status.SuccessOk
        | 201 -> Status.SuccessCreated
        | 202 -> Status.SuccessCreated
        | 203 -> Status.SuccessNonAuthoritativeInformation
        | 204 -> Status.SuccessNoContent
        | 205 -> Status.SuccessResetContent
        | 206 -> Status.SuccessPartialContent
        | 207 -> Status.SuccessMultiStatus
        | 208 -> Status.SuccessAlreadyReported
        | 226 -> Status.SuccessImUsed
        | 300 -> Status.RedirectionMultipleChoices
        | 301 -> Status.RedirectionMovedPermanently
        | 302 -> Status.RedirectionFound
        | 303 -> Status.RedirectionSeeOther
        | 304 -> Status.RedirectionNotModified
        | 305 -> Status.RedirectionUseProxy
        | 307 -> Status.RedirectionTemporaryRedirect
        | 400 -> Status.ClientErrorBadRequest
        | 401 -> Status.ClientErrorUnauthorized
        | 403 -> Status.ClientErrorForbidden
        | 404 -> Status.ClientErrorNotFound
        | 405 -> Status.ClientErrorMethodNotAllowed
        | 406 -> Status.ClientErrorNotAcceptable
        | 407 -> Status.ClientErrorProxyAuthenticated
        | 408 -> Status.ClientErrorRequestTimeout
        | 409 -> Status.ClientErrorConflict
        | 410 -> Status.ClientErrorGone
        | 411 -> Status.ClientErrorLengthRequired
        | 412 -> Status.ClientErrorPreconditionFailed
        | 413 -> Status.ClientErrorRequestEntityTooLarge
        | 414 -> Status.ClientErrorRequestUriTooLong
        | 415 -> Status.ClientErrorUnsupportedMediaType
        | 416 -> Status.ClientErrorRequestedRangeNotSatisfiable
        | 417 -> Status.ClientErrorExpectationFailed
        | 422 -> Status.ClientErrorUnprocessableEntity
        | 423 -> Status.ClientErrorLocked
        | 424 -> Status.ClientErrorFailedDependency
        | 426 -> Status.ClientErrorUpgradeRequired
        | 500 -> Status.ServerErrorInternalServerError
        | 501 -> Status.ServerErrorNotImplemented
        | 502 -> Status.ServerErrorBadGateway
        | 503 -> Status.ServerErrorServiceUnavailable
        | 504 -> Status.ServerErrorGatewayTimeout
        | 505 -> Status.ServerErrorGatewayTimeout
        | 506 -> Status.ServerErrorVariantAlsoNegotiates
        | 507 -> Status.ServerErrorInsufficientStorage
        | 508 -> Status.ServerErrorLoopDetected
        | 510 -> Status.ServerErrorNotExtended
        | _ -> { code = code; msg = msg }

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

module HttpStatus =
    let ClientErrorBadRequest = Status.ClientErrorBadRequest

    let ClientErrorConflict = Status.ClientErrorConflict

    let ClientErrorExpectaionFailed = Status.ClientErrorExpectationFailed

    let ClientErrorFailedDependency = Status.ClientErrorFailedDependency

    let ClientErrorForbidden = Status.ClientErrorForbidden

    let ClientErrorGone = Status.ClientErrorGone

    let ClientErrorLengthRequired = Status.ClientErrorLengthRequired

    let ClientErrorLocked = Status.ClientErrorLocked

    let ClientErrorMethodNotAllowed = Status.ClientErrorMethodNotAllowed

    let ClientErrorNotAcceptable = Status.ClientErrorNotAcceptable

    let ClientErrorNotFound = Status.ClientErrorNotFound

    let ClientErrorPreconditionFailed = Status.ClientErrorPreconditionFailed

    let ClientErrorProxyAuthenticated = Status.ClientErrorProxyAuthenticated

    let ClientErrorRequestEntityTooLarge = Status.ClientErrorRequestEntityTooLarge

    let ClientErrorRequestTimeout = Status.ClientErrorRequestTimeout

    let ClientErrorRequestUriTooLong = Status.ClientErrorRequestUriTooLong

    let ClientErrorRequestedRangeNotSatisfiable = Status.ClientErrorRequestedRangeNotSatisfiable

    let ClientErrorUnauthorized = Status.ClientErrorUnauthorized

    let ClientErrorUnprocessableEntity = Status.ClientErrorUnprocessableEntity

    let ClientErrorUnsupportedMediaType = Status.ClientErrorUnsupportedMediaType

    let ClientErrorUpgradeRequired = Status.ClientErrorUpgradeRequired

    let InformationalContinue = Status.InformationalContinue

    let InformationalProcessing = Status.InformationalProcessing
    
    let InformationalSwitchingProtocols = Status.InformationalSwitchingProtocols

    let RedirectionFound = Status.RedirectionFound

    let RedirectionMovedPermanently = Status.RedirectionMovedPermanently

    let RedirectionMultipleChoices = Status.RedirectionMultipleChoices

    let RedirectionNotModified = Status.RedirectionNotModified

    let RedirectionSeeOther = Status.RedirectionSeeOther

    let RedirectionTemporaryRedirect = Status.RedirectionTemporaryRedirect

    let RedirectionUseProxy = Status.RedirectionUseProxy

    let ServerErrorBadGateway = Status.ServerErrorBadGateway

    let ServerErrorGatewayTimeout = Status.ServerErrorGatewayTimeout

    let ServerErrorHttpVersionNotSupported = Status.ServerErrorHttpVersionNotSupported

    let ServerErrorInsufficientStorage = Status.ServerErrorInsufficientStorage

    let ServerErrorInternalServerError = Status.ServerErrorInternalServerError

    let ServerErrorLoopDetected = Status.ServerErrorLoopDetected

    let ServerErrorNotExtended = Status.ServerErrorNotExtended

    let ServerErrorNotImplemented = Status.ServerErrorNotImplemented

    let ServerErrorServiceUnavailable = Status.ServerErrorServiceUnavailable

    let ServerErrorVariantAlsoNegotiates = Status.ServerErrorVariantAlsoNegotiates

    let SuccessAccepted = Status.SuccessAccepted

    let SuccessAlreadyReported = Status.SuccessAlreadyReported

    let SuccessCreated = Status.SuccessCreated

    let SuccessImUsed = Status.SuccessImUsed

    let SuccessMultiStatus = Status.SuccessMultiStatus

    let SuccessNoContent = Status.SuccessNoContent

    let SuccessNonAuthoritativeInformation = Status.SuccessNonAuthoritativeInformation

    let SuccessOk = Status.SuccessOk

    let SuccessPartialContent = Status.SuccessPartialContent

    let SuccessResetContent = Status.SuccessResetContent
    
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

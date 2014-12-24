namespace FunctionalHttp.Core

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
    | Informational = 100
    | Success = 200
    | Redirection = 300
    | ClientError = 400
    | ServerError = 500 
    | SystemHttpClientError = 1000

[<AutoOpen>]  
module StatusMixins =
    type Status with 
        member this.Class 
            with get() = 
                match this.Code with  
                | x when x >= 100 && x < 200 -> StatusClass.Informational 
                | x when x >= 200 && x < 300 -> StatusClass.Success 
                | x when x >= 300 && x < 400 -> StatusClass.Redirection 
                | x when x >= 400 && x < 500 -> StatusClass.ClientError
                | x when x >= 500 && x < 600 -> StatusClass.ServerError 
                | x when x >= 1000 && x < 2000 -> StatusClass.SystemHttpClientError 
                | _ -> failwith "Invalid status code"  

module HttpStatus =
    [<CompiledName("ClientErrorBadRequest")>]
    let clientErrorBadRequest = Status.ClientErrorBadRequest

    [<CompiledName("ClientErrorConflict")>]
    let clientErrorConflict = Status.ClientErrorConflict

    [<CompiledName("ClientErrorExpectationFailed")>]
    let clientErrorExpectationFailed = Status.ClientErrorExpectationFailed

    [<CompiledName("ClientErrorFailedDependency")>]
    let clientErrorFailedDependency = Status.ClientErrorFailedDependency

    [<CompiledName("ClientErrorForbidden")>]
    let clientErrorForbidden = Status.ClientErrorForbidden

    [<CompiledName("ClientErrorGone")>]
    let clientErrorGone = Status.ClientErrorGone

    [<CompiledName("ClientErrorLengthRequired")>]
    let clientErrorLengthRequired = Status.ClientErrorLengthRequired

    [<CompiledName("ClientErrorLocked")>]
    let clientErrorLocked = Status.ClientErrorLocked

    [<CompiledName("ClientErrorMethodNotAllowed")>]
    let clientErrorMethodNotAllowed = Status.ClientErrorMethodNotAllowed

    [<CompiledName("ClientErrorNotAcceptable")>]
    let clientErrorNotAcceptable = Status.ClientErrorNotAcceptable

    [<CompiledName("ClientErrorNotFound")>]
    let clientErrorNotFound = Status.ClientErrorNotFound

    [<CompiledName("ClientErrorPreconditionFailed")>]
    let clientErrorPreconditionFailed = Status.ClientErrorPreconditionFailed

    [<CompiledName("ClientErrorProxyAuthenticated")>]
    let clientErrorProxyAuthenticated = Status.ClientErrorProxyAuthenticated

    [<CompiledName("ClientError")>]
    let ClientErrorRequestEntityTooLarge = Status.ClientErrorRequestEntityTooLarge

    [<CompiledName("ClientErrorRequestTimeout")>]
    let clientErrorRequestTimeout = Status.ClientErrorRequestTimeout

    [<CompiledName("ClientErrorRequestUriTooLong")>]
    let clientErrorRequestUriTooLong = Status.ClientErrorRequestUriTooLong

    [<CompiledName("ClientErrorRangeNotSatisfiable")>]
    let clientErrorRequestedRangeNotSatisfiable = Status.ClientErrorRequestedRangeNotSatisfiable

    [<CompiledName("ClientErrorUnauthorized")>]
    let clientErrorUnauthorized = Status.ClientErrorUnauthorized

    [<CompiledName("ClientErrorUnprocessableEntity")>]
    let clientErrorUnprocessableEntity = Status.ClientErrorUnprocessableEntity

    [<CompiledName("ClientErrorUnsupportedMediaType")>]
    let clientErrorUnsupportedMediaType = Status.ClientErrorUnsupportedMediaType

    [<CompiledName("ClientErrorUpgradeRequired")>]
    let clientErrorUpgradeRequired = Status.ClientErrorUpgradeRequired

    [<CompiledName("InformationalContinue")>]
    let informationalContinue = Status.InformationalContinue

    [<CompiledName("InformationalProcessing")>]
    let informationalProcessing = Status.InformationalProcessing

    [<CompiledName("InformationalSwitchingProtocols")>]
    let informationalSwitchingProtocols = Status.InformationalSwitchingProtocols

    [<CompiledName("RedirectionFound")>]
    let redirectionFound = Status.RedirectionFound

    [<CompiledName("RedirectionMovedPermanently")>]
    let redirectionMovedPermanently = Status.RedirectionMovedPermanently

    [<CompiledName("RedirectionMultipleChoices")>]
    let redirectionMultipleChoices = Status.RedirectionMultipleChoices

    [<CompiledName("RedirectionNotModified")>]
    let redirectionNotModified = Status.RedirectionNotModified

    [<CompiledName("RedirectionSeeOther")>]
    let redirectionSeeOther = Status.RedirectionSeeOther

    [<CompiledName("RedirectionTemporaryRedirect")>]
    let redirectionTemporaryRedirect = Status.RedirectionTemporaryRedirect

    [<CompiledName("RedirectionUseProxy")>]
    let redirectionUseProxy = Status.RedirectionUseProxy

    [<CompiledName("ServerErrorBadGateway")>]
    let serverErrorBadGateway = Status.ServerErrorBadGateway

    [<CompiledName("ServerErrorGatewayTimeout")>]
    let serverErrorGatewayTimeout = Status.ServerErrorGatewayTimeout

    [<CompiledName("ServerErrorHttpVersionNotSupported")>]
    let serverErrorHttpVersionNotSupported = Status.ServerErrorHttpVersionNotSupported

    [<CompiledName("ServerErrorInsufficientStorage")>]
    let serverErrorInsufficientStorage = Status.ServerErrorInsufficientStorage

    [<CompiledName("ServerErrorInternalServerError")>]
    let serverErrorInternalServerError = Status.ServerErrorInternalServerError

    [<CompiledName("ServerErrorLoopDetected")>]
    let serverErrorLoopDetected = Status.ServerErrorLoopDetected

    [<CompiledName("ServerErrorNotExtended")>]
    let serverErrorNotExtended = Status.ServerErrorNotExtended

    [<CompiledName("ServerErrorNotImplemented")>]
    let serverErrorNotImplemented = Status.ServerErrorNotImplemented

    [<CompiledName("ServerErrorServiceUnavailable")>]
    let serverErrorServiceUnavailable = Status.ServerErrorServiceUnavailable

    [<CompiledName("ServerErrorVariantAlsoNegotiates")>]
    let serverErrorVariantAlsoNegotiates = Status.ServerErrorVariantAlsoNegotiates

    [<CompiledName("SuccessAccepted")>]
    let successAccepted = Status.SuccessAccepted

    [<CompiledName("SuccessAlreadyReported")>]
    let successAlreadyReported = Status.SuccessAlreadyReported

    [<CompiledName("SuccessCreated")>]
    let successCreated = Status.SuccessCreated

    [<CompiledName("SuccessImUsed")>]
    let successImUsed = Status.SuccessImUsed

    [<CompiledName("SuccessMultiStatus")>]
    let successMultiStatus = Status.SuccessMultiStatus

    [<CompiledName("SuccessNoContent")>]
    let successNoContent = Status.SuccessNoContent

    [<CompiledName("SuccessNonAuthoritativeInformation")>]
    let successNonAuthoritativeInformation = Status.SuccessNonAuthoritativeInformation

    [<CompiledName("SuccessOk")>]
    let successOk = Status.SuccessOk

    [<CompiledName("SuccessPartialContent")>]
    let successPartialContent = Status.SuccessPartialContent

    [<CompiledName("SuccessResetContent")>]
    let successResetContent = Status.SuccessResetContent
    
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

    [<CompiledName("DeserializationFaile")>]
    let deserializationFailed = Status.Create(1023, "Failed to deserialize response entity")

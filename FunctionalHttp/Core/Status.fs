namespace FunctionalHttp.Core

open System.Runtime.CompilerServices

type Status = 
    private {
        code:uint16
        msg:string
    }

    member this.Code = this.code  

    member this.Message = this.msg

    override this.ToString() = sprintf "%u %s" this.Code this.Message

    static member private StandardHeaders =
        [   (100, "Contine");
            (101, "Switching Protocols");
            (102, "Processing");
            (200, "OK");
            (201, "Created");
            (202, "Accepted");
            (203, "Non-Authoritative Information");
            (204, "No Content");
            (205, "Reset Content");
            (206, "Partial Content");
            (207, "Multi-Status");
            (208, "Already Reported");
            (226, "IM Used");           
            (300, "Multiple Choices");
            (301, "Moved Permanently");
            (302, "Found");
            (303, "See Other");
            (304, "Not Modified");
            (305, "Use Proxy");
            (307, "Temporary Redirect");
            (400, "Bad Request");
            (401, "Unauthorized");
            (403, "Forbidden");
            (404, "Not Found");
            (405, "Method Not Allowed");
            (406, "Not Acceptable");
            (407, "Proxy Authentication Required");
            (408, "Request Timeout");
            (409, "Conflict");
            (410, "Gone");
            (411, "Length Required");
            (412, "Precondition Failed");
            (413, "Request Entity Too Large");
            (414, "Request-URI Too Long");
            (415, "Unsupported Media Type");
            (416, "Requested Range Not Satisfiable");
            (417, "Expectation Failed");
            (422, "Unprocessable Entity");
            (423, "Locked");
            (424, "Failed Dependency");
            (426, "Upgrade Required");
            (500, "Internal Server Error");
            (501, "Not Implemented");
            (502, "Bad Gateway");
            (503, "Service Unavailable");
            (504, "Gateway Timeout");
            (505, "HTTP Version Not Supported");
            (506, "Variant Also Negotiates");
            (507, "Insufficient Storage");
            (508, "Loop Detected");
            (510, "Not Extended"); 
            (1000, "Network Unavailable");
            (1002, "ConnectFailure");
            (1003, "ConnectionClosed");
            (1004, "KeepAliveFailure");
            (1005, "MessageLengthLimitExceeded");
            (1006, "NameResolutionFailure");
            (1008, "PipelineFailure");
            (1009, "ProtocolError");
            (1010, "ProxyNameResolutionFailure");
            (1011, "ReceiveFailure");
            (1012, "RequestCanceled");
            (1015, "SecureChannelFailure");
            (1016, "SendFailure");
            (1017, "ServerProtocolViolation");
            (1019, "Timeout");
            (1020, "TrustFailure");
            (1021, "UnknownError")]
        |> Seq.map (fun (k,v) -> (uint16 k, { code = uint16 k; msg = v }))
        |> Map.ofSeq

    static member Create (code) = Status.Create(code, "Undefined")

    static member Create (code, msg) = 
        match Status.StandardHeaders.TryFind code with
        | Some status -> status
        | _ -> { code = code; msg = msg }

type StatusClass =    
    | Informational = 100us
    | Success = 200us
    | Redirection = 300us
    | ClientError = 400us
    | ServerError = 500us
    | SystemHttpClientError = 1000us // FIXME: blah

[<AutoOpen>]  
module StatusMixins =
    type Status with 
        [<Extension>]
        member this.Class 
            with get() = 
                match this.Code with  
                | x when x >= 100us && x < 200us -> StatusClass.Informational 
                | x when x >= 200us && x < 300us -> StatusClass.Success 
                | x when x >= 300us && x < 400us -> StatusClass.Redirection 
                | x when x >= 400us && x < 500us -> StatusClass.ClientError
                | x when x >= 500us && x < 600us -> StatusClass.ServerError 
                | x when x >= 1000us && x < 2000us -> StatusClass.SystemHttpClientError 
                | _ -> failwith "Invalid status code" 

module HttpStatus =
    [<CompiledName("ClientErrorBadRequest")>]
    let clientErrorBadRequest = Status.Create(400us)

    [<CompiledName("ClientErrorConflict")>]
    let clientErrorConflict = Status.Create(409us)

    [<CompiledName("ClientErrorExpectationFailed")>]
    let clientErrorExpectationFailed = Status.Create(417us)

    [<CompiledName("ClientErrorFailedDependency")>]
    let clientErrorFailedDependency = Status.Create(424us)

    [<CompiledName("ClientErrorForbidden")>]
    let clientErrorForbidden = Status.Create(403us)

    [<CompiledName("ClientErrorGone")>]
    let clientErrorGone = Status.Create(410us)

    [<CompiledName("ClientErrorLengthRequired")>]
    let clientErrorLengthRequired = Status.Create(411us)

    [<CompiledName("ClientErrorLocked")>]
    let clientErrorLocked = Status.Create(423us)

    [<CompiledName("ClientErrorMethodNotAllowed")>]
    let clientErrorMethodNotAllowed = Status.Create(405us)

    [<CompiledName("ClientErrorNotAcceptable")>]
    let clientErrorNotAcceptable = Status.Create(406us)

    [<CompiledName("ClientErrorNotFound")>]
    let clientErrorNotFound = Status.Create(404us)

    [<CompiledName("ClientErrorPreconditionFailed")>]
    let clientErrorPreconditionFailed = Status.Create(412us)

    [<CompiledName("ClientErrorProxyAuthenticated")>]
    let clientErrorProxyAuthenticated = Status.Create(407us)

    [<CompiledName("ClientErrorRequestEntityTooLarge")>]
    let ClientErrorRequestEntityTooLarge = Status.Create(413us)

    [<CompiledName("ClientErrorRequestTimeout")>]
    let clientErrorRequestTimeout = Status.Create(408us)

    [<CompiledName("ClientErrorRequestUriTooLong")>]
    let clientErrorRequestUriTooLong = Status.Create(414us)

    [<CompiledName("ClientErrorRangeNotSatisfiable")>]
    let clientErrorRequestedRangeNotSatisfiable = Status.Create(416us)

    [<CompiledName("ClientErrorUnauthorized")>]
    let clientErrorUnauthorized = Status.Create(401us)

    [<CompiledName("ClientErrorUnprocessableEntity")>]
    let clientErrorUnprocessableEntity = Status.Create(422us)

    [<CompiledName("ClientErrorUnsupportedMediaType")>]
    let clientErrorUnsupportedMediaType = Status.Create(415us)

    [<CompiledName("ClientErrorUpgradeRequired")>]
    let clientErrorUpgradeRequired = Status.Create(426us)

    [<CompiledName("InformationalContinue")>]
    let informationalContinue = Status.Create(100us)

    [<CompiledName("InformationalProcessing")>]
    let informationalProcessing = Status.Create(102us)

    [<CompiledName("InformationalSwitchingProtocols")>]
    let informationalSwitchingProtocols = Status.Create(101us)

    [<CompiledName("RedirectionFound")>]
    let redirectionFound = Status.Create(302us)

    [<CompiledName("RedirectionMovedPermanently")>]
    let redirectionMovedPermanently = Status.Create(301us)

    [<CompiledName("RedirectionMultipleChoices")>]
    let redirectionMultipleChoices = Status.Create(300us)

    [<CompiledName("RedirectionNotModified")>]
    let redirectionNotModified = Status.Create(304us)

    [<CompiledName("RedirectionSeeOther")>]
    let redirectionSeeOther = Status.Create(303us)

    [<CompiledName("RedirectionTemporaryRedirect")>]
    let redirectionTemporaryRedirect = Status.Create(307us)

    [<CompiledName("RedirectionUseProxy")>]
    let redirectionUseProxy = Status.Create(305us)

    [<CompiledName("ServerErrorBadGateway")>]
    let serverErrorBadGateway = Status.Create(502us)

    [<CompiledName("ServerErrorGatewayTimeout")>]
    let serverErrorGatewayTimeout = Status.Create(504us)

    [<CompiledName("ServerErrorHttpVersionNotSupported")>]
    let serverErrorHttpVersionNotSupported = Status.Create(505us)

    [<CompiledName("ServerErrorInsufficientStorage")>]
    let serverErrorInsufficientStorage = Status.Create(507us)

    [<CompiledName("ServerErrorInternalServerError")>]
    let serverErrorInternalServerError = Status.Create(500us)

    [<CompiledName("ServerErrorLoopDetected")>]
    let serverErrorLoopDetected = Status.Create(508us)

    [<CompiledName("ServerErrorNotExtended")>]
    let serverErrorNotExtended = Status.Create(510us)

    [<CompiledName("ServerErrorNotImplemented")>]
    let serverErrorNotImplemented = Status.Create(501us)

    [<CompiledName("ServerErrorServiceUnavailable")>]
    let serverErrorServiceUnavailable = Status.Create(503us)

    [<CompiledName("ServerErrorVariantAlsoNegotiates")>]
    let serverErrorVariantAlsoNegotiates = Status.Create(506us)

    [<CompiledName("SuccessAccepted")>]
    let successAccepted = Status.Create(202us)

    [<CompiledName("SuccessAlreadyReported")>]
    let successAlreadyReported = Status.Create(208us)

    [<CompiledName("SuccessCreated")>]
    let successCreated = Status.Create(201us)

    [<CompiledName("SuccessImUsed")>]
    let successImUsed = Status.Create(226us)

    [<CompiledName("SuccessMultiStatus")>]
    let successMultiStatus = Status.Create(207us)

    [<CompiledName("SuccessNoContent")>]
    let successNoContent = Status.Create(204us)

    [<CompiledName("SuccessNonAuthoritativeInformation")>]
    let successNonAuthoritativeInformation = Status.Create(203us)

    [<CompiledName("SuccessOk")>]
    let successOk = Status.Create(200us)

    [<CompiledName("SuccessPartialContent")>]
    let successPartialContent = Status.Create(206us)

    [<CompiledName("SuccessResetContent")>]
    let successResetContent = Status.Create(205us)
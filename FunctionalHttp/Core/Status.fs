namespace FunctionalHttp.Core

type Status = 
    private {
        code:int
        msg:string
    }

    member this.Code = this.code  

    member this.Message = this.msg

    override this.ToString() = this.Code.ToString() + " " + this.Message

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
        |> Seq.map (fun (k,v) -> (k, { code = k; msg = v }))
        |> Map.ofSeq

    static member Create (code) = Status.Create(code, "Undefined")

    static member Create (code, msg) = 
        match Status.StandardHeaders.TryFind code with
        | Some status -> status
        | _ -> { code = code; msg = msg }

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
    let clientErrorBadRequest = Status.Create(400)

    [<CompiledName("ClientErrorConflict")>]
    let clientErrorConflict = Status.Create(409)

    [<CompiledName("ClientErrorExpectationFailed")>]
    let clientErrorExpectationFailed = Status.Create(417)

    [<CompiledName("ClientErrorFailedDependency")>]
    let clientErrorFailedDependency = Status.Create(424)

    [<CompiledName("ClientErrorForbidden")>]
    let clientErrorForbidden = Status.Create(403)

    [<CompiledName("ClientErrorGone")>]
    let clientErrorGone = Status.Create(410)

    [<CompiledName("ClientErrorLengthRequired")>]
    let clientErrorLengthRequired = Status.Create(411)

    [<CompiledName("ClientErrorLocked")>]
    let clientErrorLocked = Status.Create(423)

    [<CompiledName("ClientErrorMethodNotAllowed")>]
    let clientErrorMethodNotAllowed = Status.Create(405)

    [<CompiledName("ClientErrorNotAcceptable")>]
    let clientErrorNotAcceptable = Status.Create(406)

    [<CompiledName("ClientErrorNotFound")>]
    let clientErrorNotFound = Status.Create(404)

    [<CompiledName("ClientErrorPreconditionFailed")>]
    let clientErrorPreconditionFailed = Status.Create(412)

    [<CompiledName("ClientErrorProxyAuthenticated")>]
    let clientErrorProxyAuthenticated = Status.Create(407)

    [<CompiledName("ClientErrorRequestEntityTooLarge")>]
    let ClientErrorRequestEntityTooLarge = Status.Create(413)

    [<CompiledName("ClientErrorRequestTimeout")>]
    let clientErrorRequestTimeout = Status.Create(408)

    [<CompiledName("ClientErrorRequestUriTooLong")>]
    let clientErrorRequestUriTooLong = Status.Create(414)

    [<CompiledName("ClientErrorRangeNotSatisfiable")>]
    let clientErrorRequestedRangeNotSatisfiable = Status.Create(416)

    [<CompiledName("ClientErrorUnauthorized")>]
    let clientErrorUnauthorized = Status.Create(401)

    [<CompiledName("ClientErrorUnprocessableEntity")>]
    let clientErrorUnprocessableEntity = Status.Create(422)

    [<CompiledName("ClientErrorUnsupportedMediaType")>]
    let clientErrorUnsupportedMediaType = Status.Create(415)

    [<CompiledName("ClientErrorUpgradeRequired")>]
    let clientErrorUpgradeRequired = Status.Create(426)

    [<CompiledName("InformationalContinue")>]
    let informationalContinue = Status.Create(100)

    [<CompiledName("InformationalProcessing")>]
    let informationalProcessing = Status.Create(102)

    [<CompiledName("InformationalSwitchingProtocols")>]
    let informationalSwitchingProtocols = Status.Create(101)

    [<CompiledName("RedirectionFound")>]
    let redirectionFound = Status.Create(302)

    [<CompiledName("RedirectionMovedPermanently")>]
    let redirectionMovedPermanently = Status.Create(301)

    [<CompiledName("RedirectionMultipleChoices")>]
    let redirectionMultipleChoices = Status.Create(300)

    [<CompiledName("RedirectionNotModified")>]
    let redirectionNotModified = Status.Create(304)

    [<CompiledName("RedirectionSeeOther")>]
    let redirectionSeeOther = Status.Create(303)

    [<CompiledName("RedirectionTemporaryRedirect")>]
    let redirectionTemporaryRedirect = Status.Create(307)

    [<CompiledName("RedirectionUseProxy")>]
    let redirectionUseProxy = Status.Create(305)

    [<CompiledName("ServerErrorBadGateway")>]
    let serverErrorBadGateway = Status.Create(502)

    [<CompiledName("ServerErrorGatewayTimeout")>]
    let serverErrorGatewayTimeout = Status.Create(504)

    [<CompiledName("ServerErrorHttpVersionNotSupported")>]
    let serverErrorHttpVersionNotSupported = Status.Create(505)

    [<CompiledName("ServerErrorInsufficientStorage")>]
    let serverErrorInsufficientStorage = Status.Create(507)

    [<CompiledName("ServerErrorInternalServerError")>]
    let serverErrorInternalServerError = Status.Create(500)

    [<CompiledName("ServerErrorLoopDetected")>]
    let serverErrorLoopDetected = Status.Create(508)

    [<CompiledName("ServerErrorNotExtended")>]
    let serverErrorNotExtended = Status.Create(510)

    [<CompiledName("ServerErrorNotImplemented")>]
    let serverErrorNotImplemented = Status.Create(501)

    [<CompiledName("ServerErrorServiceUnavailable")>]
    let serverErrorServiceUnavailable = Status.Create(503)

    [<CompiledName("ServerErrorVariantAlsoNegotiates")>]
    let serverErrorVariantAlsoNegotiates = Status.Create(506)

    [<CompiledName("SuccessAccepted")>]
    let successAccepted = Status.Create(202)

    [<CompiledName("SuccessAlreadyReported")>]
    let successAlreadyReported = Status.Create(208)

    [<CompiledName("SuccessCreated")>]
    let successCreated = Status.Create(201)

    [<CompiledName("SuccessImUsed")>]
    let successImUsed = Status.Create(226)

    [<CompiledName("SuccessMultiStatus")>]
    let successMultiStatus = Status.Create(207)

    [<CompiledName("SuccessNoContent")>]
    let successNoContent = Status.Create(204)

    [<CompiledName("SuccessNonAuthoritativeInformation")>]
    let successNonAuthoritativeInformation = Status.Create(203)

    [<CompiledName("SuccessOk")>]
    let successOk = Status.Create(200)

    [<CompiledName("SuccessPartialContent")>]
    let successPartialContent = Status.Create(206)

    [<CompiledName("SuccessResetContent")>]
    let successResetContent = Status.Create(205)
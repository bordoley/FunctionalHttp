namespace FunctionalHttp

type Status = 
    private
        | Status of code:int * msg:string

    static member ClientErrorBadRequest = Status(400, "Bad Request")

    static member ClientErrorConflict = Status(409, "Conflict")

    static member ClientErrorExpectaionFailed = Status(417, "Expectation Failed")

    static member ClientErrorFailedDependency = Status(424, "Failed Dependency")

    static member ClientErrorForbidden = Status(403, "Forbidden")

    static member ClientErrorGone = Status(410, "Gone")

    static member ClientErrorLengthRequired = Status(411, "Length Required")

    static member ClientErrorLocked = Status(423, "Locked")

    static member ClientErrorMethodNotAllowed = Status(405, "Method Not Allowed")

    static member ClientErrorNotAcceptable = Status(406, "Not Acceptable")

    static member ClientErrorNotFound = Status(404, "Not Found")

    static member ClientErrorPreconditionFailed = Status(412, "Precondition Failed")

    static member ClientErrorProxyAuthenticated = Status(407, "Proxy Authentication Required")

    static member ClientErrorRequestEntityTooLarge = Status(413, "Request Entity Too Large")

    static member ClientErrorRequestTimeout = Status(408, "Request Timeout")

    static member ClientErrorRequestUriTooLong = Status(414, "Request-URI Too Long")

    static member ClientErrorRequestedRangeNotSatisfiable = Status(416, "Requested Range Not Satisfiable")

    static member ClientErrorUnauthorized = Status(401, "Unauthorized")

    static member ClientErrorUnprocessableEntity = Status(422, "Unprocessable Entity")

    static member ClientErrorUnsupportedMediaType = Status(415, "Unsupported Media Type")

    static member ClientErrorUpgradeRequired = Status(426, "Upgrade Required")

    static member InformationalContinue = Status(100, "Continue")

    static member InformationalProcessing = Status(102, "Processing")
    
    static member InformationalSwitchingProtocols = Status(101, "Switching Protocols")

    static member RedirectionFound = Status(302, "Found")

    static member RedirectionMovedPermanently = Status(301, "Moved Permanently")

    static member RedirectionMultipleChoices = Status(300, "Multiple Choices")

    static member RedirectionNotModified = Status(304, "Not Modified")

    static member RedirectionSeeOther = Status(303, "See Other")

    static member RedirectionTemporaryRedirect = Status(307, "Temporary Redirect")

    static member RedirectionUseProxy = Status(305, "Use Proxy")

    static member ServerErrorBadGateway = Status(502, "Bad Gateway")

    static member ServerErrorGatewayTimeout = Status(504, "Gateway Timeout")

    static member ServerErrorHttpVersionNotSupported = Status(505, "HTTP Version Not Supported")

    static member ServerErrorInsufficientStorage = Status(507, "Insufficient Storage")

    static member ServerErrorInternalServerError = Status(500, "Internal Server Error")

    static member ServerErrorLoopDetected = Status(508, "Loop Detected")

    static member ServerErrorNotExtended = Status(510, "Not Extended")

    static member ServerErrorNotImplemented = Status(501, "Not Implemented")

    static member ServerErrorServiceUnavailable = Status(503, "Service Unavailable")

    static member ServerErrorVariantAlsoNegotiates = Status(506, "Variant Also Negotiates")

    static member SuccessAccepted = Status(202, "Accepted")

    static member SuccessAlreadyReported = Status(208, "Already Reported")

    static member SuccessCreated = Status(201, "Created")

    static member SuccessImUsed = Status(226, "IM Used")

    static member SuccessMultiStatus = Status(207, "Multi-Status")

    static member SuccessNoContent = Status(204, "No Content")

    static member SuccessNonAuthoritativeInformation = Status(203, "Non-Authoritative Information")

    static member SuccessOk = Status(200, "OK")

    static member SuccessPartialContent = Status(206, "Partial Content")

    static member SuccessResetContent = Status(205, "Reset Content")

    static member Create code = Status(code, "") 

    member this.Code = match this with Status(code, _) -> code
    
    member this.Message = match this with Status(_, msg) -> msg

    override this.ToString() = this.Message

type StatusClass = 
    | ClientError | Informational | Redirection | ServerError | Success | SystemHttpClientError
    static member From (status:Status) =
        match status with  
        | Status(x, _) when x >= 100 && x < 200 -> StatusClass.Informational 
        | Status (x, _) when x >= 200 && x < 300 -> StatusClass.Success 
        | Status (x, _) when x >= 300 && x < 400 -> StatusClass.Redirection 
        | Status (x, _) when x >= 400 && x < 500 -> StatusClass.ClientError
        | Status (x, _) when x >= 500 && x < 600 -> StatusClass.ServerError 
        | Status (x, _) when x >= 1000 && x < 2000 -> StatusClass.SystemHttpClientError 
        | _ -> failwith "Invalid status code"  
        
module StatusExtension =
    type Status with
        member this.Class 
            with get() = StatusClass.From(this)

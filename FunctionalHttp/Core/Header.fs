namespace FunctionalHttp.Core

open System
open FunctionalHttp.Parsing

[<Sealed>]
type Header internal (header:string) = 
    static member Create(header) = 
        match Parser.parse Header.Parser header with 
        | Some header -> header
        | _ -> raise (ArgumentException("header"))

    static member internal Parser = HttpParsers.token |> Parser.map (fun header -> Header(header))

    member val private Normalized = header.ToLowerInvariant()

    interface IComparable<Header> with
        member this.CompareTo(other) = this.Normalized.CompareTo(other.Normalized)

    interface IComparable with
        member this.CompareTo(other) =
            match other with
            | :? Header as other -> (this :> IComparable<Header>).CompareTo(other)
            | _ -> invalidArg "obj" "not a Header" 

    interface IEquatable<Header> with
        member this.Equals that =
            this.Normalized = that.Normalized

    override this.Equals that =
        match that with
        | :? Header as that -> (this :> IEquatable<Header>).Equals that
        | _ -> false

    override this.GetHashCode() = hash this.Normalized

    override this.ToString () = header

module HttpHeaders =
    [<CompiledName("Accept")>]
    let accept = Header("Accept")

    [<CompiledName("AcceptCharset")>]
    let acceptCharset = Header("Accept-Charset")

    [<CompiledName("AcceptEncoding")>]
    let acceptEncoding = Header("Accept-Encoding")

    [<CompiledName("AcceptLanguage")>]
    let acceptLanguage = Header("Accept-Language")

    [<CompiledName("AcceptRanges")>]
    let acceptRanges = Header("Accept-Ranges")

    [<CompiledName("Age")>]
    let age = Header("Age")

    [<CompiledName("Allow")>]
    let allow = Header("Allow")

    [<CompiledName("Authorization")>]
    let authorization = Header("Authorization")

    [<CompiledName("CacheControl")>]
    let cacheControl = Header("Cache-Control")

    [<CompiledName("Connection")>]
    let connection = Header("Connection")

    [<CompiledName("ContentEncoding")>]
    let contentEncoding = Header("Content-Encoding")

    [<CompiledName("ContentLanguage")>]
    let contentLanguage = Header("Content-Language")

    [<CompiledName("ContentLength")>]
    let contentLength = Header("Content-Length")

    [<CompiledName("ContentLocation")>]
    let contentLocation = Header("Content-Location")

    [<CompiledName("ContentMD5")>]
    let contentMD5 = Header("Content-MD5")

    [<CompiledName("ContentRange")>]
    let contentRange = Header("Content-Range")

    [<CompiledName("ContentType")>]
    let contentType = Header("Content-Type")

    [<CompiledName("Cookie")>]
    let cookie = Header("Cookie")

    [<CompiledName("Date")>]
    let date = Header("Date")

    [<CompiledName("ETag")>]
    let etag = Header("ETag")

    [<CompiledName("Expect")>]
    let expect = Header("Expect")

    [<CompiledName("Expires")>]
    let expires = Header("Expires")

    [<CompiledName("From")>]
    let from = Header("From")

    [<CompiledName("Host")>]
    let host = Header("Host")

    [<CompiledName("ifMatch")>]
    let IfMatch = Header("If-Match")

    [<CompiledName("IfModifiedSince")>]
    let ifModifiedSince = Header("If-Modified-Since")

    [<CompiledName("IfNoneMatch")>]
    let ifNoneMatch = Header("If-None-Match")

    [<CompiledName("IfRange")>]
    let ifRange = Header("If-Range")

    [<CompiledName("IfUnmodifiedSince")>]
    let ifUnmodifiedSince = Header("If-Unmodified-Since")

    [<CompiledName("LastModified")>]
    let lastModified = Header("Last-Modified")

    [<CompiledName("Location")>]
    let location = Header("Location")

    [<CompiledName("MaxForwards")>]
    let maxForwards = Header("Max-Forwards")

    [<CompiledName("Pragma")>]
    let pragma = Header("Pragma")

    [<CompiledName("ProxyAuthenticate")>]
    let proxyAuthenticate = Header("Proxy-Authenticate")

    [<CompiledName("ProxyAuthorization")>]
    let proxyAuthorization = Header("Proxy-Authorization")

    [<CompiledName("Range")>]
    let range = Header("Range")

    [<CompiledName("Referer")>]
    let referer = Header("Referer")

    [<CompiledName("RetryAfter")>]
    let retryAfter = Header("Retry-After")

    [<CompiledName("Server")>]
    let server = Header("Server")

    [<CompiledName("SetCookie")>]
    let setCookie = Header("Set-Cookie")

    [<CompiledName("TE")>]
    let te = Header("TE")

    [<CompiledName("Trailer")>]
    let trailer = Header("Trailer")

    [<CompiledName("TransferEncoding")>]
    let transferEncoding = Header("Transfer-Encoding")

    [<CompiledName("Upgrade")>]
    let upgrade = Header("Upgrade")

    [<CompiledName("UserAgent")>]
    let userAgent = Header("User-Agent")

    [<CompiledName("Vary")>]
    let vary = Header("Vary")

    [<CompiledName("Via")>]
    let via = Header("Via")

    [<CompiledName("Warning")>]
    let warning = Header("Warning")

    [<CompiledName("WwwAuthenticate")>]
    let wwwAuthenticate = Header("WWW-Authenticate")

    // Extension Headers
    [<CompiledName("XHttpMethod ")>]
    let xHttpMethod = Header("X-HTTP-Method")

    [<CompiledName("XHttpMethodOverride")>]
    let xHttpMethodOverride = Header("X-HTTP-Method-Override")

    [<CompiledName("XMethodOverride")>]
    let xMethodOverride = Header("X-Method-Override")

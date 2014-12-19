namespace FunctionalHttp

open System

type Header private (header:string) = 
    // FIXME: need to validate the string is a valid header using a contract
    static member Create(header) = Header(header)

    static member internal Parser = HttpParsers.token |> Parser.map (fun header -> Header(header))

    static member val Accept = Header("Accept")
    static member val AcceptCharset = Header("Accept-Charset")
    static member val AcceptEncoding = Header("Accept-Encoding")
    static member val AcceptLanguage = Header("Accept-Language")
    static member val AcceptRanges = Header("Accept-Ranges")
    static member val Age = Header("Age")
    static member val Allow = Header("Allow")
    static member val Authorization = Header("Authorization")
    static member val CacheControl = Header("Cache-Control")
    static member val Connection = Header("Connection")
    static member val ContentEncoding = Header("Content-Encoding")
    static member val ContentLanguage = Header("Content-Language")
    static member val ContentLength = Header("Content-Length")
    static member val ContentLocation = Header("Content-Location")
    static member val ContentMD5 = Header("Content-MD5")
    static member val ContentRange = Header("Content-Range")
    static member val ContentType = Header("Content-Type")
    static member val Cookie = Header("Cookie")
    static member val Date = Header("Date")
    static member val ETag = Header("ETag")
    static member val Expect = Header("Expect")
    static member val Expires = Header("Expires")
    static member val From = Header("From")
    static member val Host = Header("Host")
    static member val IfMatch = Header("If-Match")
    static member val IfModifiedSince = Header("If-Modified-Since")
    static member val IfNoneMatch = Header("If-None-Match")
    static member val IfRange = Header("If-Range")
    static member val IfUnmodifiedSince = Header("If-Unmodified-Since")
    static member val LastModified = Header("Last-Modified")
    static member val Location = Header("Location")
    static member val MaxForwards = Header("Max-Forwards")
    static member val Pragma = Header("Pragma")
    static member val ProxyAuthenticate = Header("Proxy-Authenticate")
    static member val ProxyAuthorization = Header("Proxy-Authorization")
    static member val Range = Header("Range")
    static member val Referer = Header("Referer")
    static member val RetryAfter = Header("Retry-After")
    static member val Server = Header("Server")
    static member val SetCookie = Header("Set-Cookie")
    static member val TE = Header("TE")
    static member val Trailer = Header("Trailer")
    static member val TransferEncoding = Header("Transfer-Encoding")
    static member val Upgrade = Header("Upgrade")
    static member val UserAgent = Header("User-Agent")
    static member val Vary = Header("Vary")
    static member val Via = Header("Via")
    static member val Warning = Header("Warning")
    static member val WwwAuthenticate = Header("WWW-Authenticate")

    // Extension Headers
    static member val XHttpMethod = Header("X-HTTP-Method")
    static member val XHttpMethodOverride = Header("X-HTTP-Method-Override")
    static member val XMethodOverride = Header("X-Method-Override")

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

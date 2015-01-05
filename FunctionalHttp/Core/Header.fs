namespace FunctionalHttp.Core

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open FunctionalHttp.Parsing

open FunctionalHttp.Parsing.Parser

[<Sealed>]
type Header private (header:string) = 
    static let normalize (header:string) = header.ToLowerInvariant() 

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

    static member internal StandardHeaders  =
        [ Header("Accept") ;
          Header("Accept-Charset");
          Header("Accept-Encoding");
          Header("Accept-Language");
          Header("Accept-Ranges");
          Header("Age");
          Header("Allow");
          Header("Authorization");
          Header("Cache-Control");
          Header("Connection");
          Header("Content-Encoding");
          Header("Content-Language");
          Header("Content-Length");
          Header("Content-Location");
          Header("Content-MD5");
          Header("Content-Range");
          Header("Content-Type");
          Header("Cookie");
          Header("Date");
          Header("ETag");
          Header("Expect");
          Header("Expires");
          Header("From");
          Header("Host");
          Header("If-Match");
          Header("If-Modified-Since");
          Header("If-None-Match");
          Header("If-Range");
          Header("If-Unmodified-Since");
          Header("Last-Modified");
          Header("Location");
          Header("Max-Forwards");
          Header("Pragma");
          Header("Proxy-Authenticate");
          Header("Proxy-Authorization");
          Header("Range");
          Header("Referer");
          Header("Retry-After");
          Header("Server");
          Header("Set-Cookie");
          Header("TE");
          Header("Trailer");
          Header("Transfer-Encoding");
          Header("Upgrade");
          Header("User-Agent");
          Header("Vary");
          Header("Via");
          Header("Warning");
          Header("WWW-Authenticate");
          Header("X-HTTP-Method");
          Header("X-HTTP-Method-Override");
          Header("X-Method-Override")] |> Seq.map (fun x -> (x.Normalized, x)) |> Map.ofSeq
    
    static member Create (header: string) = 
        match normalize header |> Header.StandardHeaders.TryFind with
        | Some header -> header
        | None ->
            match parse Header.Parser header with 
            | Some header -> header
            | _ -> invalidArg "header" "not a header"

    static member internal Parser = 
        HttpParsers.token |>> (fun header -> 
            match normalize header |> Header.StandardHeaders.TryFind with
            | Some header -> header
            | _ ->  Header(header))

module internal HeaderInternal =
    let internal headerSet = (Header.StandardHeaders :> IDictionary<string, Header>).Values |> Set.ofSeq

    let filterStandardHeaders (headers:Map<Header,obj>) =
        headers |> Map.toSeq |> (Seq.filter <| fun (k,v) -> headerSet.Contains k |> not) |> Map.ofSeq

    let headerMapFromRawHeaders (headers:seq<string*(string seq)>) =
        // FIXME: Special case cookies
        headers 
        |> Seq.map(fun (k,v) -> (Header.Create k, String.Join (",", v) :> obj)) 
        |> Map.ofSeq

    let inline writeOption (f:string*string->unit) (k:Header, v:^T option) =
        v |> Option.map(fun v -> f (string k, string v)) |> ignore

    let writeSeq (f:string*string->unit) (k:Header, seq:IEnumerable) =
        let v = String.Join (", ", seq.Cast<obj>()) 

        if String.IsNullOrEmpty v then ()
        else f (string k, v)
    
    let writeDateTime (f:string*string->unit) (k:Header, v:DateTime option) =
        // FIXME:
        ()

    let writeDeltaSecond (f:string*string->unit) (k:Header, v:TimeSpan option) =
        v |> Option.map(fun v -> f (string k, v.Ticks / 10000000L |> string)) |> ignore

    let writeObject (f:string*string->unit) (k:Header, v:obj) =
        match v with
        // String implements IEnumerable in some profiles and not others
        | :?String as v when v.Length > 0 -> f (string k, string v) 
        | :?DateTime as v -> writeDateTime f (k, Some v)
        | :?IEnumerable as v -> writeSeq f (k, v)
        | _ -> 
            let v = string v
            if v.Length > 0 then f (string k, v)

    let writeAll (f:string*string->unit) (pairs:seq<Header*obj>) =
        pairs |> Seq.map (writeObject f) |> ignore

    let headerLineFunc builder (k:string, v:string) =
        Printf.bprintf builder "%O: %s\r\n" k v

module HttpHeaders =
    [<CompiledName("Accept")>]
    let accept = Header.Create("Accept")

    [<CompiledName("AcceptCharset")>]
    let acceptCharset = Header.Create("Accept-Charset")

    [<CompiledName("AcceptEncoding")>]
    let acceptEncoding = Header.Create("Accept-Encoding")

    [<CompiledName("AcceptLanguage")>]
    let acceptLanguage = Header.Create("Accept-Language")

    [<CompiledName("AcceptRanges")>]
    let acceptRanges = Header.Create("Accept-Ranges")

    [<CompiledName("Age")>]
    let age = Header.Create("Age")

    [<CompiledName("Allow")>]
    let allow = Header.Create("Allow")

    [<CompiledName("Authorization")>]
    let authorization = Header.Create("Authorization")

    [<CompiledName("CacheControl")>]
    let cacheControl = Header.Create("Cache-Control")

    [<CompiledName("Connection")>]
    let connection = Header.Create("Connection")

    [<CompiledName("ContentEncoding")>]
    let contentEncoding = Header.Create("Content-Encoding")

    [<CompiledName("ContentLanguage")>]
    let contentLanguage = Header.Create("Content-Language")

    [<CompiledName("ContentLength")>]
    let contentLength = Header.Create("Content-Length")

    [<CompiledName("ContentLocation")>]
    let contentLocation = Header.Create("Content-Location")

    [<CompiledName("ContentMD5")>]
    let contentMD5 = Header.Create("Content-MD5")

    [<CompiledName("ContentRange")>]
    let contentRange = Header.Create("Content-Range")

    [<CompiledName("ContentType")>]
    let contentType = Header.Create("Content-Type")

    [<CompiledName("Cookie")>]
    let cookie = Header.Create("Cookie")

    [<CompiledName("Date")>]
    let date = Header.Create("Date")

    [<CompiledName("ETag")>]
    let etag = Header.Create("ETag")

    [<CompiledName("Expect")>]
    let expect = Header.Create("Expect")

    [<CompiledName("Expires")>]
    let expires = Header.Create("Expires")

    [<CompiledName("From")>]
    let from = Header.Create("From")

    [<CompiledName("Host")>]
    let host = Header.Create("Host")

    [<CompiledName("ifMatch")>]
    let ifMatch = Header.Create("If-Match")

    [<CompiledName("IfModifiedSince")>]
    let ifModifiedSince = Header.Create("If-Modified-Since")

    [<CompiledName("IfNoneMatch")>]
    let ifNoneMatch = Header.Create("If-None-Match")

    [<CompiledName("IfRange")>]
    let ifRange = Header.Create("If-Range")

    [<CompiledName("IfUnmodifiedSince")>]
    let ifUnmodifiedSince = Header.Create("If-Unmodified-Since")

    [<CompiledName("LastModified")>]
    let lastModified = Header.Create("Last-Modified")

    [<CompiledName("Location")>]
    let location = Header.Create("Location")

    [<CompiledName("MaxForwards")>]
    let maxForwards = Header.Create("Max-Forwards")

    [<CompiledName("Pragma")>]
    let pragma = Header.Create("Pragma")

    [<CompiledName("ProxyAuthenticate")>]
    let proxyAuthenticate = Header.Create("Proxy-Authenticate")

    [<CompiledName("ProxyAuthorization")>]
    let proxyAuthorization = Header.Create("Proxy-Authorization")

    [<CompiledName("Range")>]
    let range = Header.Create("Range")

    [<CompiledName("Referer")>]
    let referer = Header.Create("Referer")

    [<CompiledName("RetryAfter")>]
    let retryAfter = Header.Create("Retry-After")

    [<CompiledName("Server")>]
    let server = Header.Create("Server")

    [<CompiledName("SetCookie")>]
    let setCookie = Header.Create("Set-Cookie")

    [<CompiledName("TE")>]
    let te = Header.Create("TE")

    [<CompiledName("Trailer")>]
    let trailer = Header.Create("Trailer")

    [<CompiledName("TransferEncoding")>]
    let transferEncoding = Header.Create("Transfer-Encoding")

    [<CompiledName("Upgrade")>]
    let upgrade = Header.Create("Upgrade")

    [<CompiledName("UserAgent")>]
    let userAgent = Header.Create("User-Agent")

    [<CompiledName("Vary")>]
    let vary = Header.Create("Vary")

    [<CompiledName("Via")>]
    let via = Header.Create("Via")

    [<CompiledName("Warning")>]
    let warning = Header.Create("Warning")

    [<CompiledName("WwwAuthenticate")>]
    let wwwAuthenticate = Header.Create("WWW-Authenticate")

    // Extension Headers
    [<CompiledName("XHttpMethod ")>]
    let xHttpMethod = Header.Create("X-HTTP-Method")

    [<CompiledName("XHttpMethodOverride")>]
    let xHttpMethodOverride = Header.Create("X-HTTP-Method-Override")

    [<CompiledName("XMethodOverride")>]
    let xMethodOverride = Header.Create("X-Method-Override")
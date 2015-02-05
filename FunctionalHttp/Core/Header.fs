namespace FunctionalHttp.Core

open System
open System.Collections
open System.Collections.Generic
open System.Linq
open Sparse

[<Sealed>]
type Header internal (header:string) = 
    member val internal Normalized = header.ToLowerInvariant()

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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Header =
    let private standardHeaders =
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
          Header("WWW-Authenticate")] |> Set.ofList

    let private cachedHeaders =
        let additionalHeaders =   
            [   Header("X-HTTP-Method");
                Header("X-HTTP-Method-Override");
                Header("X-Method-Override")] 
        Seq.concat [standardHeaders :> seq<Header>; additionalHeaders :> seq<Header>] 
        |> Seq.map (fun x -> (x.Normalized, x)) 
        |> Map.ofSeq

    let internal parser = 
        HttpParsers.token |>> (fun header -> 
            match (header.ToLowerInvariant()) |> cachedHeaders.TryFind with
            | Some header -> header
            | _ ->  Header(header))

    [<CompiledName("Create")>]
    let create (header: string) = 
        match (header.ToLowerInvariant()) |> cachedHeaders.TryFind with
        | Some header -> header
        | None ->
            match parse parser header with 
            | Success (header, _) -> header
            | _ -> invalidArg "header" "not a header"

    [<CompiledName("Accept")>]
    let accept = create("Accept")

    [<CompiledName("AcceptCharset")>]
    let acceptCharset = create("Accept-Charset")

    [<CompiledName("AcceptEncoding")>]
    let acceptEncoding = create("Accept-Encoding")

    [<CompiledName("AcceptLanguage")>]
    let acceptLanguage = create("Accept-Language")

    [<CompiledName("AcceptRanges")>]
    let acceptRanges = create("Accept-Ranges")

    [<CompiledName("Age")>]
    let age = create("Age")

    [<CompiledName("Allow")>]
    let allow = create("Allow")

    [<CompiledName("Authorization")>]
    let authorization = create("Authorization")

    [<CompiledName("CacheControl")>]
    let cacheControl = create("Cache-Control")

    [<CompiledName("Connection")>]
    let connection = create("Connection")

    [<CompiledName("ContentEncoding")>]
    let contentEncoding = create("Content-Encoding")

    [<CompiledName("ContentLanguage")>]
    let contentLanguage = create("Content-Language")

    [<CompiledName("ContentLength")>]
    let contentLength = create("Content-Length")

    [<CompiledName("ContentLocation")>]
    let contentLocation = create("Content-Location")

    [<CompiledName("ContentMD5")>]
    let contentMD5 = create("Content-MD5")

    [<CompiledName("ContentRange")>]
    let contentRange = create("Content-Range")

    [<CompiledName("ContentType")>]
    let contentType = create("Content-Type")

    [<CompiledName("Cookie")>]
    let cookie = create("Cookie")

    [<CompiledName("Date")>]
    let date = create("Date")

    [<CompiledName("ETag")>]
    let etag = create("ETag")

    [<CompiledName("Expect")>]
    let expect = create("Expect")

    [<CompiledName("Expires")>]
    let expires = create("Expires")

    [<CompiledName("From")>]
    let from = create("From")

    [<CompiledName("Host")>]
    let host = create("Host")

    [<CompiledName("ifMatch")>]
    let ifMatch = create("If-Match")

    [<CompiledName("IfModifiedSince")>]
    let ifModifiedSince = create("If-Modified-Since")

    [<CompiledName("IfNoneMatch")>]
    let ifNoneMatch = create("If-None-Match")

    [<CompiledName("IfRange")>]
    let ifRange = create("If-Range")

    [<CompiledName("IfUnmodifiedSince")>]
    let ifUnmodifiedSince = create("If-Unmodified-Since")

    [<CompiledName("LastModified")>]
    let lastModified = create("Last-Modified")

    [<CompiledName("Location")>]
    let location = create("Location")

    [<CompiledName("MaxForwards")>]
    let maxForwards = create("Max-Forwards")

    [<CompiledName("Pragma")>]
    let pragma = create("Pragma")

    [<CompiledName("ProxyAuthenticate")>]
    let proxyAuthenticate = create("Proxy-Authenticate")

    [<CompiledName("ProxyAuthorization")>]
    let proxyAuthorization = create("Proxy-Authorization")

    [<CompiledName("Range")>]
    let range = create("Range")

    [<CompiledName("Referer")>]
    let referer = create("Referer")

    [<CompiledName("RetryAfter")>]
    let retryAfter = create("Retry-After")

    [<CompiledName("Server")>]
    let server = create("Server")

    [<CompiledName("SetCookie")>]
    let setCookie = create("Set-Cookie")

    [<CompiledName("TE")>]
    let te = create("TE")

    [<CompiledName("Trailer")>]
    let trailer = create("Trailer")

    [<CompiledName("TransferEncoding")>]
    let transferEncoding = create("Transfer-Encoding")

    [<CompiledName("Upgrade")>]
    let upgrade = create("Upgrade")

    [<CompiledName("UserAgent")>]
    let userAgent = create("User-Agent")

    [<CompiledName("Vary")>]
    let vary = create("Vary")

    [<CompiledName("Via")>]
    let via = create("Via")

    [<CompiledName("Warning")>]
    let warning = create("Warning")

    [<CompiledName("WwwAuthenticate")>]
    let wwwAuthenticate = create("WWW-Authenticate")

    // Extension Headers
    [<CompiledName("XHttpMethod ")>]
    let xHttpMethod = create("X-HTTP-Method")

    [<CompiledName("XHttpMethodOverride")>]
    let xHttpMethodOverride = create("X-HTTP-Method-Override")

    [<CompiledName("XMethodOverride")>]
    let xMethodOverride = create("X-Method-Override")

    let internal filterStandardHeaders (headers:Map<Header,obj>) =
        headers |> Map.toSeq |> (Seq.filter <| fun (k,v) -> standardHeaders.Contains k |> not) |> Map.ofSeq

    let internal headerMapFromRawHeaders (headers:seq<string*(string seq)>) =
        // FIXME: Special case cookies
        headers 
        |> Seq.map(fun (k,v) -> 
            let header = create k
            if header = setCookie then (header, v :> obj)
            else (header, String.Join (",", v) :> obj)) 
        |> Map.ofSeq

    let inline writeOption (f:string*string->unit) (k:Header, v:^T option) =
        v |> Option.map(fun v -> f (string k, string v)) |> ignore

    let internal writeSeq (f:string*string->unit) (k:Header, seq:IEnumerable) =
        let v = String.Join (", ", seq.Cast<obj>()) 

        if String.IsNullOrEmpty v then ()
        else f (string k, v)
    
    let internal writeDateTime (f:string*string->unit) (k:Header, v:DateTime option) =
        match v with 
        | None -> ()
        | Some date ->
            f (string k, HttpEncoding.dateToHttpDate date)

    let internal writeDeltaSecond (f:string*string->unit) (k:Header, v:TimeSpan option) =
        v |> Option.map(fun v -> f (string k, v.Ticks / 10000000L |> string)) |> ignore

    let internal writeObject (f:string*string->unit) (k:Header, v:obj) =
        match v with
        // String implements IEnumerable in some profiles and not others
        | :?String as v when v.Length > 0 -> f (string k, string v) 
        | :?DateTime as v -> writeDateTime f (k, Some v)
        | :?IEnumerable as v -> writeSeq f (k, v)
        | _ -> 
            let v = string v
            if v.Length > 0 then f (string k, v)

    let internal writeAll (f:string*string->unit) (pairs:seq<Header*obj>) =
        pairs |> Seq.map (writeObject f) |> ignore

    let internal headerLineFunc builder (k:string, v:string) =
        Printf.bprintf builder "%O: %s\r\n" k v
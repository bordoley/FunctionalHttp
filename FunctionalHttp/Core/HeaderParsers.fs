namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open FunctionalHttp.Parsing
open System
open System.Globalization

open FunctionalHttp.Parsing.Parser

module internal HeaderParsers =
    let private parse (header, parser) (headers : Map<Header, obj>) =
        headers.TryFind header
        |> Option.bind (fun x -> 
            string x |> parse parser)

    let private parseUri header (headers : Map<Header, obj>) =
        headers.TryFind header
        |> Option.bind (fun x -> 
            try Uri(string x, UriKind.RelativeOrAbsolute) |> Some
            with | :?FormatException -> None) 

    let private parseUInt64 header (headers : Map<Header, obj>) =
        headers.TryFind header |> Option.bind (fun x -> 
            let result = ref 0UL
            if UInt64.TryParse (string x, NumberStyles.None, NumberFormatInfo.InvariantInfo, result)
            then Some !result
            else None)

    let private parseSeq (header, parser:Parser<seq<_>>) (headers : Map<Header, obj>) =
        (parse (header, parser) headers) |> Option.getOrElse Seq.empty

    let private parseNonNegativeTimeSpan header (headers : Map<Header, obj>) =
        headers.TryFind header |> Option.bind (fun x ->
            match UInt32.TryParse (string x) with
            | (true, int) -> Some (TimeSpan(10000000L * int64 int))
            | _ -> None)

    let private cacheDirectiveSeq = CacheDirective.Parser|> HttpParsers.httpList
    let private challengeSeq = Challenge.Parser |> HttpParsers.httpList

    // Request
    let authorization = parse (HttpHeaders.authorization, Credentials.Parser)
    let cacheControl headers = parseSeq (HttpHeaders.cacheControl, cacheDirectiveSeq) headers |> Set.ofSeq
    let expectContinue (headers : Map<Header, obj>) =
        headers.TryFind HttpHeaders.expect
        |> Option.map (fun x -> string x = "100-continue")
        |> Option.getOrElse false

    let pragma headers = parseSeq (HttpHeaders.pragma, cacheDirectiveSeq) headers |> Set.ofSeq
    let proxyAuthorization = parse (HttpHeaders.proxyAuthorization, Credentials.Parser)
    let referer = parseUri HttpHeaders.referer 
    let userAgent = parse (HttpHeaders.userAgent, UserAgent.Parser)

    // Response
    let private acceptRangesParser = (RangeUnit.Parser |> HttpParsers.httpList1 |>> Set.ofSeq) <^> AcceptsNone.Parser
    let acceptedRanges = parse (HttpHeaders.acceptRanges, acceptRangesParser)

    let age = parseNonNegativeTimeSpan HttpHeaders.age
    let allowed headers = parseSeq (HttpHeaders.allow, Method.Parser |> HttpParsers.httpList) headers |> Set.ofSeq
    let wwwAuthenticate headers = parseSeq (HttpHeaders.wwwAuthenticate, challengeSeq) headers |> Set.ofSeq
    //let date = None
    let etag = parse (HttpHeaders.etag, EntityTag.Parser)
    //let expires = None
    //let lastModified = None
    let location = parseUri HttpHeaders.location
    let proxyAuthenticate headers = parseSeq (HttpHeaders.proxyAuthenticate, challengeSeq) headers |> Set.ofSeq
    //let retryAfter = None
    let server = parse (HttpHeaders.server, Server.Parser)

    let private varyParser = (Header.Parser |> HttpParsers.httpList1|>> Set.ofSeq) <^> Any.Parser
    let vary = parse (HttpHeaders.vary, varyParser)
    //let warning = []

    // ContentInfo
    //let encodings:ContentCoding seq = Seq.empty
    //let languages:LanguageTag seq = Seq.empty

    let length = parseUInt64 HttpHeaders.contentLength

    let contentLocation = parseUri HttpHeaders.contentLocation

    let contentType = parse (HttpHeaders.contentType, MediaType.Parser)

    //let contentRange

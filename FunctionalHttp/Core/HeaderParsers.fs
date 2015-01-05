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
    let authorization headers = parse (HttpHeaders.authorization, Credentials.Parser) headers
    let cacheControl headers = parseSeq (HttpHeaders.cacheControl, cacheDirectiveSeq) headers |> Set.ofSeq
    let expectContinue (headers : Map<Header, obj>) =
        headers.TryFind HttpHeaders.expect
        |> Option.map (fun x -> string x = "100-continue")
        |> Option.getOrElse false

    let pragma headers = parseSeq (HttpHeaders.pragma, cacheDirectiveSeq) headers |> Set.ofSeq
    let proxyAuthorization headers = parse (HttpHeaders.proxyAuthorization, Credentials.Parser) headers
    let referer headers = parseUri HttpHeaders.referer headers
    let userAgent headers = parse (HttpHeaders.userAgent, UserAgent.Parser) headers

    // Response
    let private acceptRangesParser = (RangeUnit.Parser |> HttpParsers.httpList|>> Set.ofSeq) <^> AcceptsNone.Parser
    let acceptedRanges headers = parse (HttpHeaders.acceptRanges, acceptRangesParser) headers

    let age headers = parseNonNegativeTimeSpan HttpHeaders.age headers
    let allowed headers = parseSeq (HttpHeaders.allow, Method.Parser |> HttpParsers.httpList) headers |> Set.ofSeq
    let wwwAuthenticate headers = parseSeq (HttpHeaders.wwwAuthenticate, challengeSeq) headers |> Set.ofSeq
    //let date = None
    let etag headers = parse (HttpHeaders.etag, EntityTag.Parser) headers
    //let expires = None
    //let lastModified = None
    let location headers = parseUri HttpHeaders.location headers
    let proxyAuthenticate headers = parseSeq (HttpHeaders.proxyAuthenticate, challengeSeq) headers |> Set.ofSeq
    //let retryAfter = None
    let server headers = parse (HttpHeaders.server, Server.Parser) headers
    //let vary = None
    //let warning = []

    // ContentInfo
    //let encodings:ContentCoding seq = Seq.empty
    //let languages:LanguageTag seq = Seq.empty

    let length headers = parseUInt64 HttpHeaders.contentLength headers

    let contentLocation headers = parseUri HttpHeaders.contentLocation headers

    let contentType headers = parse (HttpHeaders.contentType, MediaType.Parser) headers

    //let contentRange

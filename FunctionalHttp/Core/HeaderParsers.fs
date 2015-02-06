namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse
open System
open System.Globalization

open Abnf
open Predicates

module internal HeaderParsers =
    let private parse (header, parser:Parser<_>) (headers : Map<Header, obj>) =
        headers.TryFind header
        |> Option.bind (fun x -> 
            (string x) |> parse parser |> function
                | Success (x, _) -> Some x
                | Fail i -> None)

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

    let private cacheDirectiveSeq = CacheDirective.parser|> HttpParsers.httpList
    let private challengeSeq = Challenge.parser |> HttpParsers.httpList

    // Request
    let authorization = parse (HttpHeaders.authorization, Challenge.parser)
    let cacheControl headers = parseSeq (HttpHeaders.cacheControl, cacheDirectiveSeq) headers |> Set.ofSeq
    let expectContinue (headers : Map<Header, obj>) =
        headers.TryFind HttpHeaders.expect
        |> Option.map (fun x -> string x = "100-continue")
        |> Option.getOrElse false

    let pragma headers = parseSeq (HttpHeaders.pragma, cacheDirectiveSeq) headers |> Set.ofSeq
    let proxyAuthorization = parse (HttpHeaders.proxyAuthorization, Challenge.parser)
    let referer = parseUri HttpHeaders.referer 
    let userAgent = parse (HttpHeaders.userAgent, UserAgent.parser)

    // Response
    let private acceptRangesParser = (RangeUnit.parser |> HttpParsers.httpList1 |>> Set.ofSeq) <^> AcceptsNone.parser
    let acceptedRanges = parse (HttpHeaders.acceptRanges, acceptRangesParser)

    let age = parseNonNegativeTimeSpan HttpHeaders.age
    let allowed headers = parseSeq (HttpHeaders.allow, Method.parser |> HttpParsers.httpList) headers |> Set.ofSeq
    let wwwAuthenticate headers = parseSeq (HttpHeaders.wwwAuthenticate, challengeSeq) headers |> Set.ofSeq
    let date = parse (HttpHeaders.date, HttpParsers.httpDate)
    let etag = parse (HttpHeaders.etag, EntityTag.parser)
    let expires = parse (HttpHeaders.expires, HttpParsers.httpDate)
    let lastModified = parse (HttpHeaders.lastModified, HttpParsers.httpDate)
    let location = parseUri HttpHeaders.location
    let proxyAuthenticate headers = parseSeq (HttpHeaders.proxyAuthenticate, challengeSeq) headers |> Set.ofSeq
    let retryAfter = 
        // FIXME: Int32.Parse can throw
        let delaySeconds = many1Satisfy isDigit |>> Int64.Parse |>> (fun x ->
            DateTime.UtcNow.AddTicks (x * 10000000L))
        let p = HttpParsers.httpDate <|> delaySeconds
        parse (HttpHeaders.retryAfter, p)

    let server = parse (HttpHeaders.server, Server.parser)

    let private varyParser = 
        // Header is a token and tchar includes "*". So try parsing "*" .>> eof first and if that fails fall back
        (Any.parser .>> eof) <^>  (Header.parser |> HttpParsers.httpList1 |>> Set.ofSeq) |>> function
            | Choice1Of2 any -> Choice2Of2 any
            | Choice2Of2 headers -> Choice1Of2 headers
    let vary = parse (HttpHeaders.vary, varyParser)
    let warning = parseSeq (HttpHeaders.warning, Warning.parser |> HttpParsers.httpList)

    // ContentInfo
    let contentEncoding = parseSeq (HttpHeaders.contentEncoding, ContentCoding.parser |> HttpParsers.httpList)

    let contentLanguages = parseSeq (HttpHeaders.contentLanguage, LanguageTag.parser |> HttpParsers.httpList)

    let contentLength = parseUInt64 HttpHeaders.contentLength

    let contentLocation = parseUri HttpHeaders.contentLocation

    let contentType = parse (HttpHeaders.contentType, MediaType.parser)

    let contentRange =
        let p = ByteContentRange.parser<^>OtherContentRange.parser
        parse (HttpHeaders.contentRange, p)

    // Request Preconditions
    let private eTagPreconditionParser = (EntityTag.parser |> HttpParsers.httpList1|>> Set.ofSeq) <^> Any.parser
    let ifMatch = parse (HttpHeaders.ifMatch, eTagPreconditionParser)
    let ifModifiedSince = parse (HttpHeaders.ifModifiedSince, HttpParsers.httpDate)
    let ifNoneMatch = parse (HttpHeaders.ifNoneMatch, eTagPreconditionParser)
    let ifUnmodifiedSince = parse (HttpHeaders.ifUnmodifiedSince, HttpParsers.httpDate)
    let ifRange =
        let p = EntityTag.parser <^> HttpParsers.httpDate
        parse (HttpHeaders.ifRange, p)

    // Request PReferences
    let accept =
        let parser = AcceptPreference.parser |> HttpParsers.httpList
        parseSeq (HttpHeaders.accept, parser)

    let acceptCharset = 
        let parser = (Preference.parser Charset.parser) |> HttpParsers.httpList
        parseSeq (HttpHeaders.acceptCharset, parser)

    let acceptEncoding = 
        let parser = (Preference.parser ContentCoding.parser) |> HttpParsers.httpList
        parseSeq (HttpHeaders.acceptEncoding, parser)

    let acceptLanguage = 
        let parser = (Preference.parser LanguageTag.parser) |> HttpParsers.httpList
        parseSeq (HttpHeaders.acceptLanguage, parser)

    let range =
        let parser = ByteRangesSpecifier.parser <^> OtherRangesSpecifier.parser
        parse (HttpHeaders.range, parser)
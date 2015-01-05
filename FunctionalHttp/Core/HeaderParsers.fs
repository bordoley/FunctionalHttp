namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open FunctionalHttp.Parsing
open System
open System.Globalization

open FunctionalHttp.Parsing.Parser

module internal HeaderParsers =
    let cacheDirectiveSeq = 
        CacheDirective.Parser|> HttpParsers.httpList

    let parse (header, parser) (headers : Map<Header, obj>) =
        headers.TryFind header
        |> Option.bind (fun x -> 
            string x |> Parser.parse parser)

    let parseUri header (headers : Map<Header, obj>) =
        headers.TryFind header
        |> Option.bind (fun x -> 
            try Uri(string x, UriKind.RelativeOrAbsolute) |> Some
            with | :?FormatException -> None) 

    let parseUInt64 header (headers : Map<Header, obj>) =
        headers.TryFind header |> Option.bind (fun x -> 
            let result = ref 0UL
            if UInt64.TryParse (string x, NumberStyles.None, NumberFormatInfo.InvariantInfo, result)
            then Some !result
            else None)

    let parseSeq (header, parser:Parser<seq<_>>) (headers : Map<Header, obj>) =
        (parse (header, parser) headers) |> Option.getOrElse Seq.empty

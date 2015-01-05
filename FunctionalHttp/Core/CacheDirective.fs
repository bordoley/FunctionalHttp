namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System
open System.Collections.Generic

open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Core.HttpParsers

type CacheDirective =
    private {
        directive:String
        value:String
    }

    member this.Directive with get() = this.Directive
    member this.Value with get() = this.value

    override this.ToString() =
        if this.value.Length = 0
            then this.Directive
        else this.directive + "=" + (HttpEncoding.asTokenOrQuotedString this.value)

    static member internal Parser =
        token .>>. ((pchar '=') .>>. (token <|> quoted_string)) |> opt

    static member NoStore = { directive = "no-store"; value  ="" }
    static member NoTransform = { directive = "no-transform"; value ="" }
    static member OnlyIfCached = { directive = "only-if-cached"; value ="" }
    static member MustRevalidate = { directive = "must-revalidate"; value ="" }
    static member Public = { directive = "public"; value ="" }
    static member ProxyRevalidate = { directive = "proxy-revalidate"; value ="" }

    static member MaxAge (value:TimeSpan) =
        { directive = "max-age"; value = (int value.TotalSeconds).ToString() }

    static member MaxStale (value:TimeSpan) =
        { directive = "max-stale"; value = (int value.TotalSeconds).ToString() }
    
    static member MinFresh (value:TimeSpan) =
        { directive = "min-fresh"; value = (int value.TotalSeconds).ToString() }

    static member SharedMaxAge (value:TimeSpan) =
        { directive = "s-maxage"; value = (int value.TotalSeconds).ToString() }

    static member NoCache (value: seq<Header>) =
        let headers = Set.ofSeq value
        { directive = "no-cache"; value = String.Join(", ", headers) }

    static member Private(value: seq<Header>) =
        let headers = Set.ofSeq value
        { directive = "private"; value = String.Join(", ", headers) }  

[<AutoOpen>]
module CacheDirectiveMixins =
    let private deltaSecondsParser = 
        (CharMatchers.many1 CharMatchers.DIGIT) |>> (fun d -> 
            match System.Int32.TryParse(d) with
            | (true, int) -> Some(int)
            | _ -> None)

    type CacheDirective with
        member this.ValueAsDeltaSeconds 
            with get() = 
                match Parser.parse deltaSecondsParser this.Value with
                | Some result -> result
                | _ -> None
        //member this.ValueAsHeaders
            

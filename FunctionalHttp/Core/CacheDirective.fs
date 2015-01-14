namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System
open System.Collections.Generic

open HttpParsers
open CharParsers

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
        token .>>. opt ((pEquals) >>. (token <|> quoted_string)) |>> function 
            | (key, Some value) -> { directive = key; value = value }
            | (key, None) -> { directive = key; value = "" }

    static member NoStore = { directive = "no-store"; value  = "" }
    static member NoTransform = { directive = "no-transform"; value = "" }
    static member OnlyIfCached = { directive = "only-if-cached"; value = "" }
    static member MustRevalidate = { directive = "must-revalidate"; value = "" }
    static member Public = { directive = "public"; value = "" }
    static member ProxyRevalidate = { directive = "proxy-revalidate"; value = "" }

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

module CacheDirectives =
    // FIXME: Add an interop extension method
    let valueAsDeltaSeconds (directive:CacheDirective) =
        match UInt32.TryParse(directive.Value) with
            | (true, int) -> Some (TimeSpan(10000000L * int64 int))
            | _ -> None
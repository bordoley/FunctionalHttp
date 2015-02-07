namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse
open System
open System.Collections.Generic
open System.Runtime.CompilerServices

open HttpParsers

type CacheDirective =
    private {
        directive:String
        value:String
    }

    member this.Directive with get() = this.directive

    member this.Value with get() = this.value

    override this.ToString() =
        if this.value.Length = 0
            then this.Directive
        else this.directive + "=" + (HttpEncoding.asTokenOrQuotedString this.value)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CacheDirective =
    let noStore = { directive = "no-store"; value  = "" }

    let noTransform = { directive = "no-transform"; value = "" }

    let onlyIfCached = { directive = "only-if-cached"; value = "" }

    let mustRevalidate = { directive = "must-revalidate"; value = "" }

    let public_ = { directive = "public"; value = "" }

    let proxyRevalidate = { directive = "proxy-revalidate"; value = "" }

    [<Extension;CompiledName("ValueAsDeltaSeconds")>]
    let valueAsDeltaSeconds (directive:CacheDirective) =
        match UInt32.TryParse(directive.Value) with
            | (true, int) -> Some (TimeSpan(10000000L * int64 int))
            | _ -> None

    let internal parser =
        token .>>. opt ((pEquals) >>. (token <|> quoted_string)) |>> fun (key, value) ->
            match (key.ToLowerInvariant(), value) with
            | ("no-store", None) -> noStore
            | ("no-transform", None) -> noTransform
            | ("only-if-cached", None) -> onlyIfCached
            | ("must-revalidate", None) -> mustRevalidate
            | ("public", None) -> public_
            | ("proxy-revalidate", None) -> proxyRevalidate
            | (key, value) -> { directive = key; value = value |> Option.getOrElse "" }

type CacheDirective with
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

    static member Private (value: seq<Header>) =
        let headers = Set.ofSeq value
        { directive = "private"; value = String.Join(", ", headers) }  

    static member NoStore = CacheDirective.noStore

    static member NoTransform = CacheDirective.noTransform

    static member OnlyIfCached = CacheDirective.onlyIfCached

    static member MustRevalidate = CacheDirective.mustRevalidate

    static member Public = CacheDirective.public_

    static member ProxyRevalidate = CacheDirective.proxyRevalidate
    
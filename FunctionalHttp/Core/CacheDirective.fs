namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse
open System
open System.Collections.Generic

open HttpParsers

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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CacheDirective =
    [<CompiledName("NoStore")>]
    let noStore = { directive = "no-store"; value  = "" }

    [<CompiledName("NoTransform")>]
    let noTransform = { directive = "no-transform"; value = "" }

    [<CompiledName("OnlyIfCached")>]
    let onlyIfCached = { directive = "only-if-cached"; value = "" }

    [<CompiledName("MustRevalidate")>]
    let mustRevalidate = { directive = "must-revalidate"; value = "" }

    [<CompiledName("Public")>]
    let public_ = { directive = "public"; value = "" }

    [<CompiledName("ProxyRevalidate")>]
    let proxyRevalidate = { directive = "proxy-revalidate"; value = "" }

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

    [<CompiledName("MaxAge")>]
    let maxAge (value:TimeSpan) =
        { directive = "max-age"; value = (int value.TotalSeconds).ToString() }

    [<CompiledName("MaxStale")>]
    let maxStale (value:TimeSpan) =
        { directive = "max-stale"; value = (int value.TotalSeconds).ToString() }

    [<CompiledName("MinFresh")>]
    let minFresh (value:TimeSpan) =
        { directive = "min-fresh"; value = (int value.TotalSeconds).ToString() }

    [<CompiledName("SharedMaxAge")>]
    let sharedMaxAge (value:TimeSpan) =
        { directive = "s-maxage"; value = (int value.TotalSeconds).ToString() }
     
    [<CompiledName("NoCache")>]
    let noCache (value: seq<Header>) =
        let headers = Set.ofSeq value
        { directive = "no-cache"; value = String.Join(", ", headers) }

    [<CompiledName("Private")>]
    let private_ (value: seq<Header>) =
        let headers = Set.ofSeq value
        { directive = "private"; value = String.Join(", ", headers) }  

    [<CompiledName("ValueAsDeltaSeconds")>]
    let valueAsDeltaSeconds (directive:CacheDirective) =
        match UInt32.TryParse(directive.Value) with
            | (true, int) -> Some (TimeSpan(10000000L * int64 int))
            | _ -> None
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

    member this.Directive with get() = this.Directive

    member this.Value with get() = this.value

    override this.ToString() =
        if this.value.Length = 0
            then this.Directive
        else this.directive + "=" + (HttpEncoding.asTokenOrQuotedString this.value)

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

    static member NoStore = CacheDirectiveHelper.NoStore

    static member NoTransform = CacheDirectiveHelper.NoTransform

    static member OnlyIfCached = CacheDirectiveHelper.OnlyIfCached

    static member MustRevalidate = CacheDirectiveHelper.MustRevalidate

    static member Public = CacheDirectiveHelper.Public

    static member ProxyRevalidate = CacheDirectiveHelper.ProxyRevalidate

and [<AbstractClass; Sealed;>] internal CacheDirectiveHelper () =
    static member val NoStore = { directive = "no-store"; value  = "" }

    static member val NoTransform = { directive = "no-transform"; value = "" }

    static member val OnlyIfCached = { directive = "only-if-cached"; value = "" }

    static member val MustRevalidate = { directive = "must-revalidate"; value = "" }

    static member val Public = { directive = "public"; value = "" }

    static member val ProxyRevalidate = { directive = "proxy-revalidate"; value = "" }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CacheDirective =
    [<Extension;CompiledName("ValueAsDeltaSeconds")>]
    let valueAsDeltaSeconds (directive:CacheDirective) =
        match UInt32.TryParse(directive.Value) with
            | (true, int) -> Some (TimeSpan(10000000L * int64 int))
            | _ -> None

    let internal parser =
        token .>>. opt ((pEquals) >>. (token <|> quoted_string)) |>> fun (key, value) ->
            match (key.ToLowerInvariant(), value) with
            | ("no-store", None) -> CacheDirective.NoStore
            | ("no-transform", None) -> CacheDirective.NoTransform
            | ("only-if-cached", None) -> CacheDirective.OnlyIfCached
            | ("must-revalidate", None) -> CacheDirective.MustRevalidate
            | ("public", None) -> CacheDirective.Public
            | ("proxy-revalidate", None) -> CacheDirective.ProxyRevalidate
            | (key, value) -> { directive = key; value = value |> Option.getOrElse "" }
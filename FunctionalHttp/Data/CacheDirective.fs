namespace FunctionalHttp

open System
open System.Collections.Generic

type CacheDirective =
    | MaxAge of value:TimeSpan
    | MaxStale of value:TimeSpan
    | MinFresh of value:TimeSpan
    | NoCache of value:(Header Set) 
    | Private of value:(Header Set)   
    | SharedMaxAge of value:TimeSpan  
    | NoStore
    | NoTransform
    | OnlyIfCached
    | MustRevalidate
    | Public
    | ProxyRevalidate
    | Extension of key:string*value:string // FIXME: Should key:Token*value:Word to be absolutely correct

    static member private maxAge = "max-age"
    static member private maxStale = "max-stale"
    static member private minFresh = "min-fresh"
    static member private noCache = "no-cache"
    static member private private_ = "private"
    static member private sharedMaxAge = "s-maxage"
    static member private noStore = "no-store"
    static member private noTransform = "no-transform"
    static member private onlyIfCached = "only-if-cached"
    static member private mustRevalidate = "must-revalidate"
    static member private public_ = "public"
    static member private proxyRevalidate = "proxy-revalidate"

    override this.ToString() =
        match this with
        | MaxAge value  -> CacheDirective.maxAge + "=" + (int32 value.TotalSeconds).ToString()
        | MaxStale value -> CacheDirective.maxStale + "=" + (int32 value.TotalSeconds).ToString()
        | MinFresh value -> CacheDirective.minFresh + "=" + (int32 value.TotalSeconds).ToString()
        | NoCache value -> CacheDirective.noCache + if value.IsEmpty then "" else "=" + String.Join(", ", value)  
        | Private value -> CacheDirective.private_ + if value.IsEmpty then "" else "=" + String.Join(", ", value)
        | SharedMaxAge value -> CacheDirective.sharedMaxAge + "=" + (int32 value.TotalSeconds).ToString()
        | NoStore -> CacheDirective.noStore
        | NoTransform -> CacheDirective.noTransform
        | OnlyIfCached -> CacheDirective.onlyIfCached
        | MustRevalidate -> CacheDirective.mustRevalidate
        | Public -> CacheDirective.public_
        | ProxyRevalidate -> CacheDirective.proxyRevalidate
        | Extension (key,value) -> key + if value.Length = 0 then "" else "=" + value

namespace FunctionalHttp

open System

type AnyTag = |AnyTag

type RequestPreconditions =
    private {
        ifMatch: Choice<AnyTag, Set<ETag>> option
        ifModifiedSince: DateTime option
        ifNoneMatch: Choice<AnyTag, Set<ETag>> option
        ifUnmodifiedSince: DateTime option
        ifRange: Choice<ETag, DateTime> option
    }
    
    static member None = { ifMatch = None; ifModifiedSince = None; ifNoneMatch = None; ifUnmodifiedSince = None; ifRange = None }

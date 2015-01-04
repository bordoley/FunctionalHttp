namespace FunctionalHttp.Core

open System

type AnyTag = |AnyTag

type RequestPreconditions =
    private {
        ifMatch: Choice<AnyTag, Set<EntityTag>> option
        ifModifiedSince: DateTime option
        ifNoneMatch: Choice<AnyTag, Set<EntityTag>> option
        ifUnmodifiedSince: DateTime option
        ifRange: Choice<EntityTag, DateTime> option
    }
    
    static member None = { ifMatch = None; ifModifiedSince = None; ifNoneMatch = None; ifUnmodifiedSince = None; ifRange = None }

    static member Create(headers:Map<Header, obj>) = 
        // FIXME:
        RequestPreconditions.None

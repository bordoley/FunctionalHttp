namespace FunctionalHttp.Core.Interop

open FunctionalHttp.Collections
open FunctionalHttp.Core
//open FunctionalHttp.Core.HttpResponseDeserializers
open System
open System.IO
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type HttpResponseExtensions private () =
    [<Extension>]
    static member ToResponse(this:Status) = Status.toResponse this

    (*
    [<Extension>]
    static member ToAsyncMemoryStreamResponse(this:HttpResponse<Stream>) = toAsyncMemoryStreamResponse this

    [<Extension>]
    static member ToAsyncByteArrayResponse(this:HttpResponse<Stream>) = toAsyncByteArrayResponse this

    [<Extension>]
    static member ToAsyncStringResponse (this:HttpResponse<Stream>) = toAsyncStringResponse this
    *)

    [<Extension>]
    static member TryGetAcceptedRange(this:HttpResponse<'TResp>, acceptedRanges : byref<AcceptableRanges>) = 
        Option.tryGetValue this.AcceptedRanges &acceptedRanges

    [<Extension>]
    static member TryGetAge(this:HttpResponse<'TResp>, age : byref<TimeSpan>) = 
        Option.tryGetValue this.Age &age

    [<Extension>]
    static member TryGetDate(this:HttpResponse<'TResp>, date : byref<DateTime>) = 
        Option.tryGetValue this.Date &date

    [<Extension>]
    static member TryGetETag(this:HttpResponse<'TResp>, etag : byref<EntityTag>) = 
        Option.tryGetValue this.ETag &etag

    [<Extension>]
    static member TryGetExpires(this:HttpResponse<'TResp>, expires : byref<DateTime>) = 
        Option.tryGetValue this.Expires &expires

    [<Extension>]
    static member TryGetLastModified(this:HttpResponse<'TResp>, lastModified : byref<DateTime>) = 
        Option.tryGetValue this.Expires &lastModified
         
    [<Extension>]
    static member TryGetLocation(this:HttpResponse<'TResp>, location : byref<Uri>) = 
        Option.tryGetValue this.Location &location

    [<Extension>]
    static member TryGetRetryAfter(this:HttpResponse<'TResp>, retryAfter : byref<DateTime>) = 
        Option.tryGetValue this.RetryAfter &retryAfter

    [<Extension>]
    static member TryGetServer(this:HttpResponse<'TResp>, server : byref<Server>) = 
        Option.tryGetValue this.Server &server
           
    [<Extension>]
    static member TryGetVary(this:HttpResponse<'TResp>, vary : byref<Vary>) = 
        Option.tryGetValue (this.Vary) &vary       
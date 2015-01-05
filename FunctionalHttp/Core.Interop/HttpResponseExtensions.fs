namespace FunctionalHttp.Core.Interop

open FunctionalHttp.Collections
open FunctionalHttp.Core
//open FunctionalHttp.Core.HttpResponseDeserializers
open System
open System.IO
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type HttpResponseExtensions private () =

    // FIXME: Prefer not to expose FSharpChoice in an interop API.
    [<Extension>]
    static member TryGetAcceptedRange(this:HttpResponse<'TResp>, acceptedRanges : byref<Choice<Set<RangeUnit>, AcceptsNone>>) = 
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
    static member TryGetVary(this:HttpResponse<'TResp>, vary : byref<Choice<Set<Header>, Any>>) = 
        Option.tryGetValue (this.Vary) &vary       
namespace FunctionalHttp.Interop

open FunctionalHttp
open FunctionalHttp.HttpStreamResponseDeserializers
open System
open System.IO
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type HttpResponseExtensions private () =
    [<Extension>]
    static member ToResponse(this:Status) = this.ToResponse()

    [<Extension>]
    static member ToAsyncResponse(this:Status) = this.ToAsyncResponse()

    [<Extension>]
    static member ToAsyncResponse(this:HttpResponse<'TResp>) = this.ToAsyncResponse()

    [<Extension>]
    static member WithoutEntityAsyncResponse<'TResp> (this:HttpResponse<_>) = this.WithoutEntityAsync<'TResp>()

    [<Extension>]
    static member ToAsyncMemoryStreamResponse(this:HttpResponse<Stream>) = toAsyncMemoryStreamResponse this

    [<Extension>]
    static member ToAsyncByteArrayResponse(this:HttpResponse<Stream>) = toAsyncByteArrayResponse this

    [<Extension>]
    static member ToAsyncStringResponse (this:HttpResponse<Stream>) = toAsyncStringResponse this

    [<Extension>]
    static member TryGetAcceptedRange(this:HttpResponse<'TResp>, acceptedRanges : byref<AcceptableRanges>) = 
        match this.AcceptedRanges with
        | None -> false
        | Some retval ->
            acceptedRanges <- retval
            true

    [<Extension>]
    static member TryGetAge(this:HttpResponse<'TResp>, age : byref<TimeSpan>) = 
        match this.Age with
        | None -> false
        | Some retval ->
            age <- retval
            true

    [<Extension>]
    static member TryGetDate(this:HttpResponse<'TResp>, date : byref<DateTime>) = 
        match this.Date with
        | None -> false
        | Some retval ->
            date <- retval
            true

    [<Extension>]
    static member TryGetEntity(this:HttpResponse<'TResp>, entity : byref<'TResp>) = 
        match this.Entity with
        | None -> false
        | Some retval ->
            entity <- retval
            true

    [<Extension>]
    static member TryGetETag(this:HttpResponse<'TResp>, etag : byref<EntityTag>) = 
        match this.ETag with
        | None -> false
        | Some retval ->
            etag <- retval
            true

    [<Extension>]
    static member TryGetExpires(this:HttpResponse<'TResp>, expires : byref<DateTime>) = 
        match this.Expires with
        | None -> false
        | Some retval ->
            expires <- retval
            true

    [<Extension>]
    static member TryGetLastModified(this:HttpResponse<'TResp>, lastModified : byref<DateTime>) = 
        match this.Expires with
        | None -> false
        | Some retval ->
            lastModified <- retval
            true
         
    [<Extension>]
    static member TryGetLocation(this:HttpResponse<'TResp>, location : byref<Uri>) = 
        match this.Location with
        | None -> false
        | Some retval ->
            location <- retval
            true

    [<Extension>]
    static member TryGetRetryAfter(this:HttpResponse<'TResp>, retryAfter : byref<DateTime>) = 
        match this.RetryAfter with
        | None -> false
        | Some retval ->
            retryAfter <- retval
            true

    [<Extension>]
    static member TryGetServer(this:HttpResponse<'TResp>, server : byref<Server>) = 
        match this.Server with
        | None -> false
        | Some retval ->
            server <- retval
            true
           
    [<Extension>]
    static member TryGetVary(this:HttpResponse<'TResp>, vary : byref<Vary>) = 
        match this.Vary with
        | None -> false
        | Some retval ->
            vary <- retval
            true
           
   
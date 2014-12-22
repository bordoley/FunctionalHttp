namespace FunctionalHttp.Interop

open FunctionalHttp
open System
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type HttpRequestExtensions private () = 
    [<Extension>]
    static member TryGetAuthorization(this:HttpRequest<'TReq>, authorization : byref<Challenge>) = 
        match this.Authorization with
        | None -> false
        | Some retval ->
            authorization <- retval
            true;
   
    [<Extension>]
    static member TryGetEntity(this:HttpRequest<'TReq>, entity : byref<'TReq>) = 
        match this.Entity with
        | None -> false
        | Some retval ->
            entity <- retval
            true;

    [<Extension>]
    static member TryGetProxyAuthorization(this:HttpRequest<'TReq>, authorization : byref<Challenge>) = 
        match this.ProxyAuthorization with
        | None -> false
        | Some retval ->
            authorization <- retval
            true;

    [<Extension>]
    static member TryGetReferer(this:HttpRequest<'TReq>, referer : byref<Uri>) = 
        match this.Referer with
        | None -> false
        | Some retval ->
            referer <- retval
            true;
   
    [<Extension>]
    static member TryGetUserAgent(this:HttpRequest<'TReq>, userAgent : byref<UserAgent>) = 
        match this.UserAgent with
        | None -> false
        | Some retval ->
            userAgent <- retval
            true;
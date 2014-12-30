namespace FunctionalHttp.Core.Interop

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type HttpRequestExtensions private () = 
    [<Extension>]
    static member TryGetAuthorization(this:HttpRequest<'TReq>, authorization : byref<Challenge>) = 
        Option.tryGetValue this.Authorization &authorization

    [<Extension>]
    static member TryGetProxyAuthorization(this:HttpRequest<'TReq>, authorization : byref<Challenge>) = 
        Option.tryGetValue this.ProxyAuthorization &authorization

    [<Extension>]
    static member TryGetReferer(this:HttpRequest<'TReq>, referer : byref<Uri>) = 
        Option.tryGetValue this.Referer &referer
   
    [<Extension>]
    static member TryGetUserAgent(this:HttpRequest<'TReq>, userAgent : byref<UserAgent>) = 
        Option.tryGetValue this.UserAgent &userAgent
namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse
open System
open System.Collections.Generic
open System.Runtime.CompilerServices
open System.Text

type HttpRequest<'TReq> = 
    private {  
        authorization:Option<Credentials>
        cacheControl:Set<CacheDirective>
        contentInfo:ContentInfo
        entity:'TReq
        expectContinue:bool
        headers:Map<Header,obj>
        id:Guid
        meth:Method 
        pragma:Set<CacheDirective>
        preconditions:RequestPreconditions
        preferences:RequestPreferences
        proxyAuthorization:Option<Credentials>
        referer:Option<Uri>
        uri:Uri
        userAgent:Option<UserAgent>
        version:HttpVersion 
    }

    member this.Authorization with get() = this.authorization
    member this.CacheControl with get() = this.cacheControl
    member this.ContentInfo with get() = this.contentInfo
    member this.Entity with get() = this.entity
    member this.ExpectContinue with get() = this.expectContinue
    member this.Id with get() = this.id
    member this.Headers with get() = this.headers
    member this.Method with get() = this.meth
    member this.Pragma with get() = this.pragma
    member this.Preconditions with get() = this.preconditions
    member this.Preferences with get() = this.preferences
    member this.ProxyAuthorization with get() = this.proxyAuthorization
    member this.Referer with get() = this.referer
    member this.Uri with get() = this.uri     
    member this.UserAgent with get() = this.userAgent
    member this.Version with get() = this.version

    override this.ToString() = 
        let builder = StringBuilder()
        let writeHeaderLine = Header.headerLineFunc builder

        Printf.bprintf builder "%O %O %O\r\n" this.Method this.Uri.PathAndQuery this.Version
        this |> HttpRequest.WriteHeaders writeHeaderLine

        string builder

    static member internal Create(  authorization,
                                    cacheControl,
                                    contentInfo,
                                    entity,
                                    expectContinue,
                                    headers,
                                    id,
                                    meth,
                                    pragma,
                                    preconditions,
                                    preferences,
                                    proxyAuthorization,
                                    referer,
                                    uri,
                                    userAgent,
                                    version) =
        {
            authorization = authorization
            cacheControl = cacheControl
            contentInfo = contentInfo
            entity = entity
            expectContinue = expectContinue
            headers = headers
            id = id
            meth = meth 
            pragma = pragma
            preconditions = preconditions
            preferences = preferences
            proxyAuthorization = proxyAuthorization
            referer = referer
            uri = uri
            userAgent = userAgent
            version = version
        }
                                   
    static member Create (  meth:Method, 
                            uri:Uri,
                            entity:'TReq,                             
                            ?authorization:Credentials, 
                            ?cacheControl:seq<CacheDirective>, 
                            ?contentInfo:ContentInfo, 
                            ?expectContinue:bool, 
                            ?headers:Map<Header,obj>,
                            ?id:Guid, 
                            ?pragma:seq<CacheDirective>, 
                            ?preconditions:RequestPreconditions, 
                            ?preferences:RequestPreferences, 
                            ?proxyAuthorization:Credentials,
                            ?referer:Uri, 
                            ?userAgent:UserAgent, 
                            ?version:HttpVersion) =
        HttpRequest<'TReq>.Create (
            authorization,
            Set.ofSeq <| defaultArg cacheControl Seq.empty,
            defaultArg contentInfo ContentInfo.None,
            entity,
            defaultArg expectContinue false,
            defaultArg headers Map.empty,
            defaultArg id (Guid.NewGuid()),
            meth, 
            Set.ofSeq <| defaultArg pragma Seq.empty,
            defaultArg preconditions RequestPreconditions.None,
            defaultArg preferences RequestPreferences.None,
            proxyAuthorization,
            referer,           
            uri, 
            userAgent,
            defaultArg version HttpVersion.Http1_1)

    static member internal Create(meth:Method, uri:Uri, version:HttpVersion, headers:Map<Header, obj>, entity:'TReq, ?id) =
        let authorization = HeaderParsers.authorization headers
        let cacheControl= HeaderParsers.cacheControl headers

        let contentInfo = ContentInfo.Create headers

        let expectContinue = HeaderParsers.expectContinue headers
        let pragma = HeaderParsers.pragma headers

        let preconditions = RequestPreconditions.Create headers
        let preferences = RequestPreferences.Create headers

        let proxyAuthorization = HeaderParsers.proxyAuthorization headers
        let referer = HeaderParsers.referer headers
        let userAgent = HeaderParsers.userAgent headers

        let headers = Header.filterStandardHeaders headers

        HttpRequest<'TReq>.Create (
            authorization,
            cacheControl,
            contentInfo,
            entity,
            expectContinue,
            headers,
            Guid.NewGuid(),
            meth, 
            pragma,
            preconditions,
            preferences,
            proxyAuthorization,
            referer,           
            uri, 
            userAgent,
            version)

    static member internal WriteHeaders (f:Header*string -> unit) (req:HttpRequest<'TReq>) =
        (HttpHeaders.host,               req.Uri.Authority)      |> Header.writeObject f
        (HttpHeaders.authorization,      req.Authorization)      |> Header.writeOption f
        (HttpHeaders.cacheControl,       req.CacheControl )      |> Header.writeSeq f 
       
        req.ContentInfo |> ContentInfo.WriteHeaders f

        (HttpHeaders.expect,             "100-continue"   )      |> fun x -> if req.ExpectContinue then (x |> Header.writeObject f)
        (HttpHeaders.pragma,             req.Pragma       )      |> Header.writeSeq f

        req.Preconditions |> RequestPreconditions.WriteHeaders f
        req.preferences |> RequestPreferences.WriteHeaders f

        (HttpHeaders.proxyAuthorization, req.ProxyAuthorization) |> Header.writeOption f
        (HttpHeaders.referer,            req.Referer           ) |> Header.writeOption f
        (HttpHeaders.userAgent,          req.UserAgent         ) |> Header.writeOption f

        req.Headers |> Map.toSeq |> Header.writeAll f

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal HttpRequestInternal =
    [<CompiledName("With")>]
    let with_<'TReq, 'TNew> (   authorization,
                                cacheControl,
                                contentInfo, 
                                entity:'TNew,
                                expectContinue,
                                headers,
                                id,
                                meth, 
                                pragma,
                                preconditions, 
                                preferences,
                                proxyAuthorization,
                                referer,
                                uri,
                                userAgent,
                                version) (req:HttpRequest<'TReq>) =
        HttpRequest<'TNew>.Create(
            Option.orElse req.Authorization authorization,
            Set.ofSeq <| defaultArg cacheControl (req.CacheControl :> CacheDirective seq),
            defaultArg contentInfo req.ContentInfo,
            entity,
            defaultArg expectContinue req.ExpectContinue,
            defaultArg headers req.Headers,
            defaultArg id req.Id,
            defaultArg meth req.Method,
            Set.ofSeq <| defaultArg pragma (req.Pragma :> CacheDirective seq),
            defaultArg preconditions req.Preconditions,
            defaultArg preferences req.Preferences,
            Option.orElse req.ProxyAuthorization proxyAuthorization,
            Option.orElse req.Referer referer,
            defaultArg uri req.Uri,
            Option.orElse req.UserAgent userAgent,
            defaultArg version req.Version)

    [<CompiledName("Without")>]
    let without (   authorization, 
                    cacheControl, 
                    contentInfo, 
                    headers,
                    pragma, 
                    preconditions, 
                    preferences, 
                    proxyAuthorization, 
                    referer, 
                    userAgent) (req:HttpRequest<'TReq>)=
        HttpRequest<'TReq>.Create(  
            (if authorization then None else req.Authorization),
            Set.ofSeq <| (if cacheControl then Seq.empty else (req.CacheControl :> CacheDirective seq)),
            (if contentInfo then ContentInfo.None else req.ContentInfo),
            req.Entity,
            req.ExpectContinue,
            (if headers then Map.empty else req.Headers),
            req.Id,
            req.Method,
            Set.ofSeq <| (if pragma then Seq.empty else (req.Pragma :> CacheDirective seq)),
            (if preconditions then RequestPreconditions.None else req.Preconditions),
            (if preferences then RequestPreferences.None else req.Preferences),
            (if proxyAuthorization then None else  req.ProxyAuthorization),
            (if referer then None else req.Referer),
            req.Uri,
            (if userAgent then None else req.UserAgent),
            req.Version)

[<AutoOpen>]
module HttpRequestExtensions =
    type HttpRequest<'TReq> with
        member this.With(   ?authorization:Credentials,
                            ?cacheControl: CacheDirective seq,
                            ?contentInfo:ContentInfo,
                            ?expectContinue: bool,
                            ?headers: Map<Header, obj>,
                            ?id: Guid,
                            ?meth:Method, 
                            ?pragma : CacheDirective seq,
                            ?preconditions: RequestPreconditions, 
                            ?preferences: RequestPreferences,
                            ?proxyAuthorization:Credentials,
                            ?referer:Uri,
                            ?uri:Uri, 
                            ?userAgent:UserAgent,
                            ?version:HttpVersion) =
            this |> HttpRequestInternal.with_  (authorization,
                                                cacheControl,
                                                contentInfo,
                                                this.Entity,
                                                expectContinue,
                                                headers,
                                                id,
                                                meth,
                                                pragma,
                                                preconditions,
                                                preferences,
                                                proxyAuthorization,
                                                referer,
                                                uri,
                                                userAgent,
                                                version)

        member this.With<'TNew> (   entity:'TNew,
                                    ?authorization:Credentials,
                                    ?cacheControl: CacheDirective seq,
                                    ?contentInfo:ContentInfo,
                                    ?expectContinue: bool,
                                    ?headers: Map<Header, obj>,
                                    ?id: Guid,
                                    ?meth:Method, 
                                    ?pragma : CacheDirective seq,
                                    ?preconditions: RequestPreconditions, 
                                    ?preferences: RequestPreferences,
                                    ?proxyAuthorization:Credentials,
                                    ?referer:Uri,
                                    ?uri:Uri, 
                                    ?userAgent:UserAgent,
                                    ?version:HttpVersion) =
            this |> HttpRequestInternal.with_ ( authorization,
                                                cacheControl,
                                                contentInfo,
                                                entity,
                                                expectContinue,
                                                headers,
                                                id,
                                                meth,
                                                pragma,
                                                preconditions,
                                                preferences,
                                                proxyAuthorization,
                                                referer,
                                                uri,
                                                userAgent,
                                                version)

        member this.Without(?authorization:bool, 
                            ?cacheControl:bool, 
                            ?contentInfo:bool, 
                            ?headers:bool,
                            ?pragma:bool, 
                            ?preconditions:bool, 
                            ?preferences:bool, 
                            ?proxyAuthorization:bool, 
                            ?referer:bool, 
                            ?userAgent:bool) =
            this |> HttpRequestInternal.without (   defaultArg authorization false, 
                                                    defaultArg cacheControl false, 
                                                    defaultArg contentInfo false, 
                                                    defaultArg headers false,
                                                    defaultArg pragma false, 
                                                    defaultArg preconditions false, 
                                                    defaultArg preferences false, 
                                                    defaultArg proxyAuthorization false, 
                                                    defaultArg referer false, 
                                                    defaultArg userAgent false) 


        [<Extension>]
        member this.TryGetAuthorization(authorization : byref<Challenge>) = 
            Option.tryGetValue this.Authorization &authorization

        [<Extension>]
        member this.ryGetProxyAuthorization(authorization : byref<Challenge>) = 
            Option.tryGetValue this.ProxyAuthorization &authorization

        [<Extension>]
        member this.ryGetReferer(referer : byref<Uri>) = 
            Option.tryGetValue this.Referer &referer
       
        [<Extension>]
        member this.TryGetUserAgent(userAgent : byref<UserAgent>) = 
            Option.tryGetValue this.UserAgent &userAgent 
                                                         
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module HttpRequest =
    [<CompiledName("Convert")>]
    let convert (converter:FunctionalHttp.Core.Converter<'TIn,'TOut>) (req:HttpRequest<'TIn>) = 
        async {
            let! result = converter (req.ContentInfo, req.Entity) |> Async.Catch
            return 
                match result with
                | Choice1Of2 (contentInfo, out) -> req.With(Choice1Of2 out, contentInfo = contentInfo)
                | Choice2Of2 exn -> req.With(Choice2Of2 exn)
        }
    
    [<CompiledName("ConvertOrThrow")>]
    let convertOrThrow (converter:FunctionalHttp.Core.Converter<'TIn,'TOut>) (req:HttpRequest<'TIn>) = 
        async {
            let! (contentInfo, out) = converter (req.ContentInfo, req.Entity)
            return req.With(out, contentInfo = contentInfo)
        }

    [<CompiledName("WithAuthorization")>]
    let withAuthorization (credentials:Credentials) (req:HttpRequest<_>) = 
        req.With(authorization = credentials)

    [<CompiledName("WithCacheControl")>]
    let withCacheControl (cacheControl:Set<CacheDirective>) (req:HttpRequest<_>) = 
        req.With(cacheControl = cacheControl)

    [<CompiledName("WithContentInfo")>]
    let withContentInfo (contentInfo:ContentInfo) (req:HttpRequest<_>) = 
        req.With(contentInfo = contentInfo)
 
    [<CompiledName("WithEntity")>]
    let withEntity (entity:'TNew) (req:HttpRequest<'TReq>) =
        req.With(entity)

    [<CompiledName("WithExpectContinue")>]
    let withExpectContinue (expectContinue:bool) (req:HttpRequest<_>) =
        req.With(expectContinue = expectContinue)

    [<CompiledName("WithHeaders")>]
    let withHeaders (headers:Map<Header,obj>) (req:HttpRequest<_>) =
        req.With(headers = headers)

    [<CompiledName("WithId")>]
    let withId (id:Guid) (req:HttpRequest<_>) =
        req.With(id = id)

    [<CompiledName("WithMethod")>]
    let withMethod (meth:Method) (req:HttpRequest<_>) =
        req.With(meth = meth)
  
    [<CompiledName("WithPragma")>]
    let withPragma (pragma:seq<CacheDirective>) (req:HttpRequest<_>) =
        req.With(pragma = pragma)

    [<CompiledName("WithPreconditions")>]
    let withPreconditions (preconditions:RequestPreconditions) (req:HttpRequest<_>) =
        req.With(preconditions = preconditions)

    [<CompiledName("WithPreferences")>]
    let withPreferences (preferences:RequestPreferences) (req:HttpRequest<_>) =
        req.With(preferences = preferences)

    [<CompiledName("WithProxyAuthorization")>]
    let withProxyAuthorization (credentials:Credentials) (req:HttpRequest<_>) = 
        req.With(proxyAuthorization = credentials)

    [<CompiledName("WithReferer")>]
    let withReferer (referer:Uri) (req:HttpRequest<_>) = 
        req.With(referer = referer)

    [<CompiledName("WithUri")>]
    let withUri (uri:Uri) (req:HttpRequest<_>) = 
        req.With(uri = uri)
     
    [<CompiledName("WithUserAgent")>]
    let withUserAgent (userAgent:UserAgent) (req:HttpRequest<_>) =
        req.With(userAgent = userAgent)

    [<CompiledName("WithVersion")>]
    let withVersion (version:HttpVersion) (req:HttpRequest<_>) =
        req.With(version = version)
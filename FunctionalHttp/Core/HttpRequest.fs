namespace FunctionalHttp

open System
open System.Runtime.CompilerServices

type HttpRequest<'TReq> = 
    private {  
        authorization:Option<Credentials>
        cacheControl:Set<CacheDirective>
        contentInfo:ContentInfo
        entity:Option<'TReq>
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
                            ?authorization:Credentials, 
                            ?cacheControl:seq<CacheDirective>, 
                            ?contentInfo:ContentInfo, 
                            ?entity:'TReq, 
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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module HttpRequest =
    [<CompiledName("With")>]
    let with_ (request:HttpRequest<'TReq>) (authorization,
                                            cacheControl,
                                            contentInfo, 
                                            entity:Option<'TNew>,
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
        HttpRequest<'TNew>.Create(
            Option.orElse request.Authorization authorization,
            Set.ofSeq <| defaultArg cacheControl (request.CacheControl :> CacheDirective seq),
            defaultArg contentInfo request.ContentInfo,
            entity,
            defaultArg expectContinue request.ExpectContinue,
            defaultArg headers request.Headers,
            defaultArg id request.Id,
            defaultArg meth request.Method,
            Set.ofSeq <| defaultArg pragma (request.Pragma :> CacheDirective seq),
            defaultArg preconditions request.Preconditions,
            defaultArg preferences request.Preferences,
            Option.orElse request.ProxyAuthorization proxyAuthorization,
            Option.orElse request.Referer referer,
            defaultArg uri request.Uri,
            Option.orElse request.UserAgent userAgent,
            defaultArg version request.Version)

    [<CompiledName("Without")>]
    let without (request:HttpRequest<'TReq>) (entity:Option<'TNew>) (authorization, 
                                                                        cacheControl, 
                                                                        contentInfo, 
                                                                        headers,
                                                                        pragma, 
                                                                        preconditions, 
                                                                        preferences, 
                                                                        proxyAuthorization, 
                                                                        referer, 
                                                                        userAgent) =
        HttpRequest<'TNew>.Create(  
            (if authorization then None else request.Authorization),
            Set.ofSeq <| (if cacheControl then Seq.empty else (request.CacheControl :> CacheDirective seq)),
            (if contentInfo then ContentInfo.None else request.ContentInfo),
            entity,
            request.ExpectContinue,
            (if headers then Map.empty else request.Headers),
            request.Id,
            request.Method,
            Set.ofSeq <| (if pragma then Seq.empty else (request.Pragma :> CacheDirective seq)),
            (if preconditions then RequestPreconditions.None else request.Preconditions),
            (if preferences then RequestPreferences.None else request.Preferences),
            (if proxyAuthorization then None else  request.ProxyAuthorization),
            (if referer then None else request.Referer),
            request.Uri,
            (if userAgent then None else request.UserAgent),
            request.Version)

[<AutoOpen>]
module HttpRequestMixins =
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
            HttpRequest.with_ this (authorization,
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

        member this.With (  entity:'TNew, 
                            ?authorization:Credentials,
                            ?cacheControl: CacheDirective seq,
                            ?contentInfo:ContentInfo, 
                            ?expectContinue: bool,
                            ?headers:Map<Header,obj>,
                            ?id: Guid,
                            ?meth:Method, 
                            ?pragma: CacheDirective seq,
                            ?preconditions: RequestPreconditions, 
                            ?preferences: RequestPreferences,
                            ?proxyAuthorization:Credentials,
                            ?referer:Uri,
                            ?uri:Uri,
                            ?userAgent:UserAgent,
                            ?version:HttpVersion) =
            HttpRequest.with_ this (authorization,
                                    cacheControl,
                                    contentInfo,
                                    Some entity,
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
        
        member this.Without(?authorization, 
                            ?cacheControl, 
                            ?contentInfo, 
                            ?headers,
                            ?pragma, 
                            ?preconditions, 
                            ?preferences, 
                            ?proxyAuthorization, 
                            ?referer, 
                            ?userAgent) =
            HttpRequest.without this this.Entity (  Option.isSome authorization, 
                                                    Option.isSome cacheControl, 
                                                    Option.isSome contentInfo, 
                                                    Option.isSome headers,
                                                    Option.isSome pragma, 
                                                    Option.isSome preconditions, 
                                                    Option.isSome preferences, 
                                                    Option.isSome proxyAuthorization, 
                                                    Option.isSome referer, 
                                                    Option.isSome userAgent)                                                           

        member this.WithoutEntity<'TNew>(  ?authorization, 
                                            ?cacheControl, 
                                            ?contentInfo, 
                                            ?headers,
                                            ?pragma, 
                                            ?preconditions, 
                                            ?preferences, 
                                            ?proxyAuthorization, 
                                            ?referer,
                                            ?userAgent) =
            HttpRequest.without this (None:Option<'TNew>) ( Option.isSome authorization, 
                                                            Option.isSome cacheControl, 
                                                            Option.isSome contentInfo, 
                                                            Option.isSome headers,
                                                            Option.isSome pragma, 
                                                            Option.isSome preconditions, 
                                                            Option.isSome preferences, 
                                                            Option.isSome proxyAuthorization, 
                                                            Option.isSome referer, 
                                                            Option.isSome userAgent)

        member this.WithoutEntityAsync<'TNew> () = async { return this.WithoutEntity<'TNew>() }
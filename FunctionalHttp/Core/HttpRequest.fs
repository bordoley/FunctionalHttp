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


[<AutoOpen>]
module HttpRequestMixins =
    type HttpRequest<'TReq> with
        member this.With<'TReq>(?authorization:Credentials,
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
            HttpRequest<'TReq>.Create(
                Option.orElse this.Authorization authorization,
                Set.ofSeq <| defaultArg cacheControl (this.CacheControl :> CacheDirective seq),
                defaultArg contentInfo this.ContentInfo,
                this.Entity,
                defaultArg expectContinue this.ExpectContinue,
                defaultArg headers this.Headers,
                defaultArg id this.Id,
                defaultArg meth this.Method,
                Set.ofSeq <| defaultArg pragma (this.Pragma :> CacheDirective seq),
                defaultArg preconditions this.Preconditions,
                defaultArg preferences this.Preferences,
                Option.orElse this.ProxyAuthorization proxyAuthorization,
                Option.orElse this.Referer referer,
                defaultArg uri this.Uri,
                Option.orElse this.UserAgent userAgent,
                defaultArg version this.Version)

        member this.With<'TNew> (entity:'TNew, 
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
            HttpRequest<'TNew>.Create(
                Option.orElse this.Authorization authorization,
                Set.ofSeq <| defaultArg cacheControl (this.CacheControl :> CacheDirective seq),
                defaultArg contentInfo this.ContentInfo,
                Some entity,
                defaultArg expectContinue this.ExpectContinue,
                defaultArg headers this.Headers,
                defaultArg id this.Id,
                defaultArg meth this.Method,
                Set.ofSeq <| defaultArg pragma (this.Pragma :> CacheDirective seq),
                defaultArg preconditions this.Preconditions,
                defaultArg preferences this.Preferences,
                Option.orElse this.ProxyAuthorization proxyAuthorization,
                Option.orElse this.Referer referer,
                defaultArg uri this.Uri,
                Option.orElse this.UserAgent userAgent,
                defaultArg version this.Version)
        
        member this.Without<'TReq>(?authorization, 
                                    ?cacheControl, 
                                    ?contentInfo, 
                                    ?headers,
                                    ?pragma, 
                                    ?preconditions, 
                                    ?preferences, 
                                    ?proxyAuthorization, 
                                    ?referer, 
                                    ?userAgent) =
            HttpRequest<'TReq>.Create(  
                (if Option.isSome authorization then None else this.Authorization),
                Set.ofSeq <| (if Option.isSome cacheControl then Seq.empty else (this.CacheControl :> CacheDirective seq)),
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                this.Entity,
                this.ExpectContinue,
                (if Option.isSome headers then Map.empty else this.Headers),
                this.Id,
                this.Method,
                Set.ofSeq <| (if Option.isSome pragma then Seq.empty else (this.Pragma :> CacheDirective seq)),
                (if Option.isSome preconditions then RequestPreconditions.None else this.Preconditions),
                (if Option.isSome preferences then RequestPreferences.None else this.Preferences),
                (if Option.isSome proxyAuthorization then None else  this.ProxyAuthorization),
                (if Option.isSome referer then None else this.Referer),
                this.Uri,
                (if Option.isSome userAgent then None else this.UserAgent),
                this.Version)

        member this.WithoutEntity<'TNew>(?contentInfo, 
                                            ?authorization, 
                                            ?cacheControl, 
                                            ?headers,
                                            ?pragma, 
                                            ?preconditions, 
                                            ?preferences, 
                                            ?proxyAuthorization, 
                                            ?referer,
                                            ?userAgent) =
            HttpRequest<'TNew>.Create(
                (if Option.isSome authorization then None else  this.Authorization),
                Set.ofSeq <| (if Option.isSome cacheControl then Seq.empty else (this.CacheControl :> CacheDirective seq)),
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                None,
                this.ExpectContinue,
                (if Option.isSome headers then Map.empty else this.Headers),
                this.Id,
                this.Method,
                Set.ofSeq <| (if Option.isSome pragma then Seq.empty else (this.Pragma :> CacheDirective seq)),
                (if Option.isSome preconditions then RequestPreconditions.None else this.Preconditions),
                (if Option.isSome preferences then RequestPreferences.None else this.Preferences),
                (if Option.isSome proxyAuthorization then None else  this.ProxyAuthorization),
                (if Option.isSome referer then None else this.Referer),
                this.Uri,
                (if Option.isSome userAgent then None else this.UserAgent),
                this.Version)

        member this.WithoutEntityAsync<'TReq> () = async { return this.WithoutEntity<'TReq>() }
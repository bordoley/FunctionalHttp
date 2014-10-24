namespace FunctionalHttp

open System
open System.Runtime.CompilerServices

[<Sealed>]
type HttpRequest<'TReq> internal (meth:Method, 
                                    uri:Uri, 
                                    entity:Option<'TReq>,
                                    authorization:Option<ChallengeMessage>,
                                    cacheControl: Set<CacheDirective>,
                                    contentInfo: ContentInfo, 
                                    expectContinue: bool,
                                    headers:Map<Header,obj>,
                                    id:Guid,
                                    pragma: Set<CacheDirective>,
                                    preconditions:RequestPreconditions,
                                    preferences:RequestPreferences,
                                    proxyAuthorization:Option<ChallengeMessage>,
                                    referer:Option<Uri>,
                                    userAgent:Option<UserAgent>,
                                    version:HttpVersion) =  
                                   
    static member Create<'TReq> (meth, 
                                    uri, 
                                    entity, 
                                    ?authorization, 
                                    ?cacheControl, 
                                    ?contentInfo, 
                                    ?expectContinue, 
                                    ?headers,
                                    ?id, 
                                    ?pragma, 
                                    ?preconditions, 
                                    ?preferences, 
                                    ?proxyAuthorization,
                                    ?referer, 
                                    ?userAgent, 
                                    ?version) =
        new HttpRequest<'TReq> (
            meth, 
            uri, 
            Some entity,
            authorization,
            Set.ofSeq <| defaultArg cacheControl Seq.empty,
            defaultArg contentInfo ContentInfo.None,
            defaultArg expectContinue false,
            defaultArg headers Map.empty,
            defaultArg id (Guid.NewGuid()),
            Set.ofSeq <| defaultArg pragma Seq.empty,
            defaultArg preconditions RequestPreconditions.None,
            defaultArg preferences RequestPreferences.None,
            proxyAuthorization,
            referer,
            userAgent,
            defaultArg version HttpVersion.Http1_1)

    static member Create<'TReq> (meth, 
                                    uri, 
                                    ?authorization, 
                                    ?cacheControl, 
                                    ?contentInfo, 
                                    ?expectContinue,
                                    ?headers,
                                    ?id, 
                                    ?pragma, 
                                    ?preconditions, 
                                    ?preferences, 
                                    ?proxyAuthorization,
                                    ?referer, 
                                    ?userAgent, 
                                    ?version) =
        new HttpRequest<'TReq>(
            meth,
            uri, 
            None,
            authorization,
            Set.ofSeq <| defaultArg cacheControl Seq.empty,
            defaultArg contentInfo ContentInfo.None,
            defaultArg expectContinue false,
            defaultArg headers Map.empty,
            defaultArg id (Guid.NewGuid()),
            Set.ofSeq <| defaultArg pragma Seq.empty,
            defaultArg preconditions RequestPreconditions.None,
            defaultArg preferences RequestPreferences.None,
            proxyAuthorization,
            referer,
            userAgent,
            defaultArg version HttpVersion.Http1_1)

    member this.Authorization with get() = authorization
    member this.CacheControl with get() = cacheControl
    member this.ContentInfo with get() = contentInfo
    member this.Entity with get() = entity
    member this.ExpectContinue with get() = expectContinue
    member this.Id with get() = id
    member this.Headers with get() = headers
    member this.Method with get() = meth
    member this.Pragma with get() = pragma
    member this.Preconditions with get() = preconditions
    member this.Preferences with get() = preferences
    member this.ProxyAuthorization with get() = proxyAuthorization
    member this.Referer with get() = referer
    member this.Uri with get() = uri     
    member this.UserAgent with get() = userAgent
    member this.Version with get() = version

[<AutoOpen>]
module HttpRequestMixins =
    type HttpRequest<'TReq> with
        member this.With<'TReq>(?meth:Method, 
                                ?uri:Uri, 
                                ?authorization:ChallengeMessage,
                                ?cacheControl: CacheDirective seq,
                                ?contentInfo:ContentInfo, 
                                ?expectContinue: bool,
                                ?headers: Map<Header, obj>,
                                ?id: Guid,
                                ?pragma : CacheDirective seq,
                                ?preconditions: RequestPreconditions, 
                                ?preferences: RequestPreferences,
                                ?proxyAuthorization:ChallengeMessage,
                                ?referer:Uri,
                                ?userAgent:UserAgent,
                                ?version:HttpVersion) =
            new HttpRequest<'TReq>(
                defaultArg meth this.Method,
                defaultArg uri this.Uri,
                this.Entity,
                (if Option.isSome authorization then authorization else this.Authorization),
                Set.ofSeq <| defaultArg cacheControl (this.CacheControl :> seq<CacheDirective>),
                defaultArg contentInfo this.ContentInfo,
                defaultArg expectContinue this.ExpectContinue,
                defaultArg headers Map.empty,
                defaultArg id this.Id,
                Set.ofSeq <| defaultArg pragma (this.Pragma :> seq<CacheDirective>),
                defaultArg preconditions this.Preconditions,
                defaultArg preferences this.Preferences,
                (if Option.isSome proxyAuthorization then proxyAuthorization else this.ProxyAuthorization),
                (if Option.isSome referer then referer else this.Referer),
                (if Option.isSome userAgent then userAgent else this.UserAgent),
                defaultArg version this.Version)

        member this.With<'TNew> (entity:'TNew, 
                                    ?meth:Method, 
                                    ?uri:Uri,
                                    ?authorization:ChallengeMessage,
                                    ?cacheControl: CacheDirective seq,
                                    ?contentInfo:ContentInfo, 
                                    ?expectContinue: bool,
                                    ?headers:Map<Header,obj>,
                                    ?id: Guid,
                                    ?pragma: CacheDirective seq,
                                    ?preconditions: RequestPreconditions, 
                                    ?preferences: RequestPreferences,
                                    ?proxyAuthorization:ChallengeMessage,
                                    ?referer:Uri,
                                    ?userAgent:UserAgent,
                                    ?version:HttpVersion) =
            new HttpRequest<'TNew>(
                defaultArg meth this.Method,
                defaultArg uri this.Uri,
                Some entity,
                (if Option.isSome authorization then authorization else this.Authorization),
                Set.ofSeq <| defaultArg cacheControl (this.CacheControl :> seq<CacheDirective>),
                defaultArg contentInfo this.ContentInfo,
                defaultArg expectContinue this.ExpectContinue,
                defaultArg headers Map.empty,
                defaultArg id this.Id,
                Set.ofSeq <| defaultArg pragma (this.Pragma :> seq<CacheDirective>),
                defaultArg preconditions this.Preconditions,
                defaultArg preferences this.Preferences,
                (if Option.isSome proxyAuthorization then proxyAuthorization else this.ProxyAuthorization),
                (if Option.isSome referer then referer else this.Referer),
                (if Option.isSome userAgent then userAgent else this.UserAgent),
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
            new HttpRequest<'TReq>(
                this.Method,
                this.Uri,
                this.Entity,
                (if Option.isSome authorization then None else  this.Authorization),
                Set.ofSeq <| (if Option.isSome cacheControl then Seq.empty else (this.CacheControl :> seq<CacheDirective>)),
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                this.ExpectContinue,
                (if Option.isSome headers then Map.empty else this.Headers),
                this.Id,
                Set.ofSeq <| (if Option.isSome pragma then Seq.empty else (this.Pragma :> seq<CacheDirective>)),
                (if Option.isSome preconditions then RequestPreconditions.None else this.Preconditions),
                (if Option.isSome preferences then RequestPreferences.None else this.Preferences),
                (if Option.isSome proxyAuthorization then None else  this.ProxyAuthorization),
                (if Option.isSome referer then None else this.Referer),
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
            new HttpRequest<'TNew>(
                this.Method,
                this.Uri,
                None,
                (if Option.isSome authorization then None else  this.Authorization),
                Set.ofSeq <| (if Option.isSome cacheControl then Seq.empty else (this.CacheControl :> seq<CacheDirective>)),
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                this.ExpectContinue,
                (if Option.isSome headers then Map.empty else this.Headers),
                this.Id,
                Set.ofSeq <| (if Option.isSome pragma then Seq.empty else (this.Pragma :> seq<CacheDirective>)),
                (if Option.isSome preconditions then RequestPreconditions.None else this.Preconditions),
                (if Option.isSome preferences then RequestPreferences.None else this.Preferences),
                (if Option.isSome proxyAuthorization then None else  this.ProxyAuthorization),
                (if Option.isSome referer then None else this.Referer),
                (if Option.isSome userAgent then None else this.UserAgent),
                this.Version)

        member this.WithoutEntityAsync<'TReq> () = async { return this.WithoutEntity<'TReq>() }

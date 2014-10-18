namespace FunctionalHttp

open System
open System.Runtime.CompilerServices

[<Sealed>]
type HttpRequest<'TReq> internal (meth:Method, 
                                    uri:Uri, 
                                    entity:Option<'TReq>,
                                    id:Guid,
                                    contentInfo: ContentInfo, 
                                    authorizationCredentials:Option<ChallengeMessage>,
                                    cacheDirectives: Set<CacheDirective>,
                                    referer:Option<Uri>) =  
                                   
    static member Create<'TReq> (meth, uri, entity, ?id, ?contentInfo, ?authorizationCredentials, ?cacheDirectives, ?referer) =
        new HttpRequest<'TReq> (
            meth, 
            uri, 
            (Some entity),
            (defaultArg id (Guid.NewGuid())),
            (defaultArg contentInfo ContentInfo.None),
            authorizationCredentials,
            Set.ofSeq <| defaultArg cacheDirectives Seq.empty,
            referer)

    static member Create<'TReq> (meth, uri, ?id, ?contentInfo, ?authorizationCredentials, ?cacheDirectives, ?referer) =
        new HttpRequest<'TReq>(
            meth,
            uri, 
            None,
            (defaultArg id (Guid.NewGuid())),
            (defaultArg contentInfo ContentInfo.None),
            authorizationCredentials,
            Set.ofSeq <| defaultArg cacheDirectives Seq.empty,
            referer)

    member this.AuthorizationCredentials with get() = authorizationCredentials
    member this.CacheDirectives with get() = cacheDirectives :> seq<CacheDirective>
    member this.ContentInfo with get() = contentInfo
    member this.Entity with get() = entity
    member this.Id with get() = id
    member this.Method with get() = meth
    member this.Referer with get() = referer
    member this.Uri with get() = uri     

[<AutoOpen>]
module HttpRequestMixins =
    type HttpRequest<'TReq> with
        member this.With<'TReq>(?meth:Method, 
                                ?uri:Uri, 
                                ?id: Guid,
                                ?contentInfo:ContentInfo, 
                                ?authorizationCredentials:ChallengeMessage,
                                ?cacheDirectives: CacheDirective seq,
                                ?referer:Uri) =
            new HttpRequest<'TReq>(
                defaultArg meth this.Method,
                defaultArg uri this.Uri,
                this.Entity,
                defaultArg id this.Id,
                defaultArg contentInfo this.ContentInfo,
                (if Option.isSome authorizationCredentials then authorizationCredentials else this.AuthorizationCredentials),
                Set.ofSeq <| defaultArg cacheDirectives this.CacheDirectives,
                (if Option.isSome referer then referer else this.Referer))

        member this.With<'TNew> (entity:'TNew, 
                                    ?meth:Method, 
                                    ?uri:Uri,
                                    ?id:Guid,
                                    ?contentInfo:ContentInfo, 
                                    ?authorizationCredentials:ChallengeMessage,
                                    ?cacheDirectives: CacheDirective seq,
                                    ?referer:Uri) =
            new HttpRequest<'TNew>(
                defaultArg meth this.Method,
                defaultArg uri this.Uri,
                Some entity,
                defaultArg id this.Id,
                (defaultArg contentInfo this.ContentInfo),
                (if Option.isSome authorizationCredentials then authorizationCredentials else this.AuthorizationCredentials),
                Set.ofSeq <| defaultArg cacheDirectives this.CacheDirectives,
                (if Option.isSome referer then referer else this.Referer))
        
        member this.Without<'TReq>(?contentInfo, ?authorizationCredentials, ?cacheDirectives, ?referer) =
            new HttpRequest<'TReq>(
                this.Method,
                this.Uri,
                this.Entity,
                this.Id,
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                (if Option.isSome authorizationCredentials then None else  this.AuthorizationCredentials),
                Set.ofSeq <| (if Option.isSome cacheDirectives then Seq.empty else this.CacheDirectives),
                (if Option.isSome referer then None else this.Referer))

        member this.WithoutEntity<'TNew>(?contentInfo, ?authorizationCredentials, ?cacheDirectives, ?referer) =
            new HttpRequest<'TNew>(
                this.Method,
                this.Uri,
                None,
                this.Id,
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                (if Option.isSome authorizationCredentials then None else  this.AuthorizationCredentials),
                Set.ofSeq <| (if Option.isSome cacheDirectives then Seq.empty else this.CacheDirectives),
                (if Option.isSome referer then None else this.Referer))

        member this.WithoutEntityAsync<'TReq> () = async { return this.WithoutEntity<'TReq>() }

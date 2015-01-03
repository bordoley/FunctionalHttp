namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open System
open System.Collections.Generic
open System.Runtime.CompilerServices

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

    static member internal Create(meth:Method, uri:Uri, version:HttpVersion, entity:'TReq, headers:IEnumerable<String*IEnumerable<String>>, ?id) =
            HttpRequest<'TReq>.Create (
                Option.None,
                Set.empty,
                ContentInfo.None,
                entity,
                false,
                Map.empty,
                Guid.NewGuid(),
                meth, 
                Set.empty,
                RequestPreconditions.None,
                RequestPreferences.None,
                Option.None,
                Option.None,           
                uri, 
                Option.None,
                version)

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
            return req.With(Choice1Of2 out, contentInfo = contentInfo)
        }

    [<CompiledName("WithEntity")>]
    let withEntity (entity:'TNew) (req:HttpRequest<'TReq>) =
        req.With(entity)
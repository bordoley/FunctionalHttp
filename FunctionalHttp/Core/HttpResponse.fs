namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open System
open System.Collections.Generic
open System.IO
open System.Text

type Vary =
    | Headers of Header seq
    | Any

type HttpResponse<'TResp> =
    private {
        acceptedRanges:Option<AcceptableRanges>
        age:Option<TimeSpan>
        allowed:Set<Method>
        authenticate:Set<Challenge>
        cacheControl: Set<CacheDirective>
        contentInfo:ContentInfo
        date:Option<DateTime>
        entity:'TResp
        etag:Option<EntityTag>
        expires:Option<DateTime>
        headers:Map<Header,obj>
        id:Guid
        lastModified:Option<DateTime>
        location:Option<Uri>
        proxyAuthenticate:Set<Challenge>
        retryAfter:Option<DateTime>
        server:Option<Server>
        status:Status
        vary:Option<Vary>
        version:HttpVersion
        warning:Warning list
    }

    member this.AcceptedRanges with get() = this.acceptedRanges
    member this.Age with get() = this.age
    member this.Allowed with get() = this.allowed
    member this.Authenticate with get() = this.authenticate
    member this.CacheControl with get() = this.cacheControl
    member this.ContentInfo with get() = this.contentInfo
    member this.Date with get() = this.date
    member this.Entity with get() = this.entity
    member this.ETag with get() = this.etag
    member this.Expires with get() = this.expires
    member this.Headers with get() = this.headers
    member this.Id with get() = this.id
    member this.LastModified with get() = this.lastModified
    member this.Location with get() = this.location
    member this.ProxyAuthenticate with get() = this.proxyAuthenticate
    member this.RetryAfter with get() = this.retryAfter
    member this.Server with get() = this.server
    member this.Status with get() = this.status
    member this.Vary with get() = this.vary
    member this.Version with get() = this.version
    member this.Warning with get() = this.warning

    static member internal Create(  acceptedRanges,
                                    age,
                                    allowed,
                                    authenticate,
                                    cacheControl,
                                    contentInfo,
                                    date,
                                    entity,
                                    etag,
                                    expires,
                                    headers,
                                    id,
                                    lastModified,
                                    location,
                                    proxyAuthenticate,
                                    retryAfter,
                                    server,
                                    status,
                                    vary,
                                    version,
                                    warning) =
        {
            acceptedRanges = acceptedRanges
            age = age
            allowed = allowed
            authenticate = authenticate
            cacheControl = cacheControl
            contentInfo = contentInfo
            date = date
            entity = entity
            etag = etag
            expires = expires
            headers = headers
            id = id
            lastModified = lastModified
            location = location
            proxyAuthenticate = proxyAuthenticate
            retryAfter = retryAfter
            server = server
            status = status
            vary = vary
            version = version
            warning = warning
        }

    static member Create(   status,
                            entity,
                            ?acceptedRanges,
                            ?age, 
                            ?allowed, 
                            ?authenticate,
                            ?cacheControl, 
                            ?contentInfo, 
                            ?date,
                            ?etag, 
                            ?expires, 
                            ?headers,
                            ?id, 
                            ?lastModified, 
                            ?location, 
                            ?proxyAuthenticate,
                            ?retryAfter,
                            ?server,
                            ?vary,
                            ?version,
                            ?warning) =
        HttpResponse<'TResp>.Create(
            age,
            acceptedRanges,
            Set.ofSeq <| defaultArg allowed Seq.empty,
            Set.ofSeq <| defaultArg authenticate Seq.empty,
            Set.ofSeq <| defaultArg cacheControl Seq.empty,
            defaultArg contentInfo ContentInfo.None,
            date,
            entity,
            etag,
            expires,
            defaultArg headers Map.empty,
            defaultArg id (Guid.NewGuid()),
            lastModified,
            location,
            Set.ofSeq <| defaultArg proxyAuthenticate Seq.empty,
            retryAfter,
            server,
            status,
            vary,
            defaultArg version HttpVersion.Http1_1,
            List.ofSeq <| defaultArg warning Seq.empty)

    static member internal Create(status, entity, headers:IEnumerable<String*String>, ?id) =
        HttpResponse<'TResp>.Create(   
            None,
            None, //age,
            Set.empty, //allowed
            Set.empty,
            Set.empty, //cacheControl,
            ContentInfo.None, //ContentInfo.Create(headers),
            None,
            entity, 
            None,
            None, //expires
            Map.empty,
            defaultArg id (Guid.NewGuid()),
            None,
            None, //location
            Set.empty,
            None,
            None,
            status,
            None,
            HttpVersion.Http1_1,
            []) 

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal HttpResponseInternal =
    [<CompiledName("With")>]
    let with_<'TResp, 'TNew> (  acceptedRanges,
                                age, 
                                allowed,
                                authenticate,
                                cacheControl, 
                                contentInfo, 
                                date,
                                entity:'TNew,
                                etag,
                                expires, 
                                headers,
                                id, 
                                lastModified,
                                location, 
                                proxyAuthenticate,
                                retryAfter,
                                server,
                                status,
                                vary,
                                version,
                                warning) (resp:HttpResponse<'TResp>) =
        HttpResponse<'TNew>.Create(
            Option.orElse resp.AcceptedRanges acceptedRanges,
            Option.orElse resp.Age age,
            Set.ofSeq <| defaultArg allowed (resp.Allowed :> Method seq),
            Set.ofSeq <| defaultArg authenticate (resp.Authenticate :> Challenge seq),
            Set.ofSeq <| defaultArg cacheControl (resp.CacheControl :> CacheDirective seq),
            defaultArg contentInfo resp.ContentInfo,
            Option.orElse resp.Date date,
            entity,
            Option.orElse resp.ETag etag,
            Option.orElse resp.Expires expires,
            defaultArg headers resp.Headers,
            defaultArg id resp.Id,
            Option.orElse resp.LastModified lastModified,
            Option.orElse resp.Location location,
            Set.ofSeq <| defaultArg proxyAuthenticate (resp.ProxyAuthenticate :> Challenge seq),
            Option.orElse resp.RetryAfter retryAfter,
            Option.orElse resp.Server server,
            defaultArg status resp.Status,
            Option.orElse resp.Vary vary,
            defaultArg version resp.Version,
            List.ofSeq <| defaultArg warning (resp.Warning :> Warning seq))

    [<CompiledName("Without")>]
    let without (   acceptedRanges,
                    age, 
                    allowed,
                    authenticate, 
                    cacheControl, 
                    contentInfo, 
                    date, 
                    etag, 
                    expires, 
                    headers, 
                    lastModified, 
                    location, 
                    proxyAuthenticate,
                    retryAfter,
                    server,
                    vary,
                    warning) (resp:HttpResponse<'TResp>) =
        HttpResponse<'TResp>.Create(
            (if acceptedRanges then None else resp.AcceptedRanges),
            (if age then None else resp.Age),
            (if allowed then Set.empty else resp.Allowed),
            (if authenticate then Set.empty else resp.Authenticate),
            (if cacheControl then Set.empty else resp.CacheControl),
            (if contentInfo then ContentInfo.None else resp.ContentInfo),
            (if date then None else resp.Date),
            resp.Entity,
            (if etag then None else resp.ETag),
            (if expires then None else resp.Expires),
            (if headers then Map.empty else resp.Headers),
            resp.Id,
            (if lastModified then None else resp.LastModified),
            (if location then None else resp.Location),
            (if proxyAuthenticate then Set.empty else resp.ProxyAuthenticate),
            (if retryAfter then None else resp.RetryAfter),
            (if server then None else resp.Server),
            resp.Status,
            (if vary then None else resp.Vary),
            resp.Version,
            (if warning then [] else resp.Warning))

[<AutoOpen>]
module HttpResponseMixins =
    type HttpResponse<'TResp> with
        member this.With (  ?acceptedRanges:AcceptableRanges,
                            ?age:TimeSpan, 
                            ?allowed: Method seq,
                            ?authenticate:Challenge seq,
                            ?cacheControl:CacheDirective seq, 
                            ?contentInfo:ContentInfo, 
                            ?date:DateTime,
                            ?etag:EntityTag,
                            ?expires:DateTime, 
                            ?headers: Map<Header, obj>,
                            ?id:Guid, 
                            ?lastModified:DateTime,
                            ?location:Uri, 
                            ?proxyAuthenticate:Challenge seq,
                            ?retryAfter:DateTime,
                            ?server:Server,
                            ?status:Status,
                            ?vary: Vary,
                            ?version:HttpVersion,
                            ?warning: Warning seq) =
            this |> HttpResponseInternal.with_ (acceptedRanges,
                                                age, 
                                                allowed,
                                                authenticate,
                                                cacheControl, 
                                                contentInfo, 
                                                date,
                                                this.Entity,
                                                etag,
                                                expires, 
                                                headers,
                                                id, 
                                                lastModified,
                                                location, 
                                                proxyAuthenticate,
                                                retryAfter,
                                                server,
                                                status,
                                                vary,
                                                version,
                                                warning)

        member this.With (  entity:'TNew,
                            ?acceptedRanges:AcceptableRanges,
                            ?age:TimeSpan, 
                            ?allowed: Method seq,
                            ?authenticate:Challenge seq,
                            ?cacheControl:CacheDirective seq, 
                            ?contentInfo:ContentInfo, 
                            ?date:DateTime,
                            ?etag:EntityTag,
                            ?expires:DateTime, 
                            ?headers,
                            ?id:Guid, 
                            ?lastModified:DateTime,
                            ?location:Uri, 
                            ?proxyAuthenticate:Challenge seq,
                            ?retryAfter:DateTime,
                            ?server:Server,
                            ?status:Status,
                            ?vary: Vary,
                            ?version:HttpVersion,
                            ?warning: Warning seq) =
            this |> HttpResponseInternal.with_ (acceptedRanges,
                                                age, 
                                                allowed,
                                                authenticate,
                                                cacheControl, 
                                                contentInfo, 
                                                date,
                                                entity,
                                                etag,
                                                expires, 
                                                headers,
                                                id, 
                                                lastModified,
                                                location, 
                                                proxyAuthenticate,
                                                retryAfter,
                                                server,
                                                status,
                                                vary,
                                                version,
                                                warning) 

        member this.Without<'TResp>(?acceptedRanges:bool,
                                    ?age:bool, 
                                    ?allowed:bool, 
                                    ?authenticate:bool, 
                                    ?cacheControl:bool, 
                                    ?contentInfo:bool, 
                                    ?date:bool, 
                                    ?etag:bool, 
                                    ?expires:bool, 
                                    ?headers:bool, 
                                    ?lastModified:bool, 
                                    ?location:bool,
                                    ?proxyAuthenticate:bool,
                                    ?retryAfter:bool, 
                                    ?server:bool,
                                    ?vary:bool,
                                    ?warning:bool) =
            this |> HttpResponseInternal.without (  defaultArg acceptedRanges false,
                                                    defaultArg age false, 
                                                    defaultArg allowed false,
                                                    defaultArg authenticate false, 
                                                    defaultArg cacheControl false, 
                                                    defaultArg contentInfo false, 
                                                    defaultArg date false, 
                                                    defaultArg etag false, 
                                                    defaultArg expires false, 
                                                    defaultArg headers false, 
                                                    defaultArg lastModified false, 
                                                    defaultArg location false, 
                                                    defaultArg proxyAuthenticate false,
                                                    defaultArg retryAfter false,
                                                    defaultArg server false,
                                                    defaultArg vary false,
                                                    defaultArg warning false) 

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module HttpResponse =
    let toObjResponse<'TResp when 'TResp : not struct> (resp:HttpResponse<'TResp>) =
        resp.With(resp.Entity :> obj)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Status =
    let toResponse (status:Status) = HttpResponse<String>.Create(status, status.ToString())
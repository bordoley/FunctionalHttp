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
        entity:Option<'TResp>
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
                            ?acceptedRanges,
                            ?age, 
                            ?allowed, 
                            ?authenticate,
                            ?cacheControl, 
                            ?contentInfo, 
                            ?date,
                            ?entity:'TResp, 
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
            Some entity, 
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
module internal HttpResponse =
    [<CompiledName("With")>]
    let with_ (response:HttpResponse<'TResp>) ( acceptedRanges,
                                                age, 
                                                allowed,
                                                authenticate,
                                                cacheControl, 
                                                contentInfo, 
                                                date,
                                                entity:Option<'TNew>,
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
        HttpResponse<'TNew>.Create(
            Option.orElse response.AcceptedRanges acceptedRanges,
            Option.orElse response.Age age,
            Set.ofSeq <| defaultArg allowed (response.Allowed :> Method seq),
            Set.ofSeq <| defaultArg authenticate (response.Authenticate :> Challenge seq),
            Set.ofSeq <| defaultArg cacheControl (response.CacheControl :> CacheDirective seq),
            defaultArg contentInfo response.ContentInfo,
            Option.orElse response.Date date,
            entity,
            Option.orElse response.ETag etag,
            Option.orElse response.Expires expires,
            defaultArg headers response.Headers,
            defaultArg id response.Id,
            Option.orElse response.LastModified lastModified,
            Option.orElse response.Location location,
            Set.ofSeq <| defaultArg proxyAuthenticate (response.ProxyAuthenticate :> Challenge seq),
            Option.orElse response.RetryAfter retryAfter,
            Option.orElse response.Server server,
            defaultArg status response.Status,
            Option.orElse response.Vary vary,
            defaultArg version response.Version,
            List.ofSeq <| defaultArg warning (response.Warning :> Warning seq))

    [<CompiledName("Without")>]
    let without (response:HttpResponse<_>) (entity:Option<'TNew>) ( acceptedRanges,
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
                                                                    warning) =
        HttpResponse<'TNew>.Create(
            (if acceptedRanges then None else response.AcceptedRanges),
            (if age then None else response.Age),
            (if allowed then Set.empty else response.Allowed),
            (if authenticate then Set.empty else response.Authenticate),
            (if cacheControl then Set.empty else response.CacheControl),
            (if contentInfo then ContentInfo.None else response.ContentInfo),
            (if date then None else response.Date),
            entity,
            (if etag then None else response.ETag),
            (if expires then None else response.Expires),
            (if headers then Map.empty else response.Headers),
            response.Id,
            (if lastModified then None else response.LastModified),
            (if location then None else response.Location),
            (if proxyAuthenticate then Set.empty else response.ProxyAuthenticate),
            (if retryAfter then None else response.RetryAfter),
            (if server then None else response.Server),
            response.Status,
            (if vary then None else response.Vary),
            response.Version,
            (if warning then [] else response.Warning))


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
            HttpResponse.with_ this (   acceptedRanges,
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
            HttpResponse.with_ this (   acceptedRanges,
                                        age, 
                                        allowed,
                                        authenticate,
                                        cacheControl, 
                                        contentInfo, 
                                        date,
                                        Some entity,
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

        member this.Without<'TResp>(?acceptedRanges,
                                    ?age, 
                                    ?allowed, 
                                    ?authenticate, 
                                    ?cacheControl, 
                                    ?contentInfo, 
                                    ?date, 
                                    ?etag, 
                                    ?expires, 
                                    ?headers, 
                                    ?lastModified, 
                                    ?location,
                                    ?proxyAuthenticate,
                                    ?retryAfter, 
                                    ?server,
                                    ?vary,
                                    ?warning) =
            HttpResponse.without this this.Entity ( Option.isSome acceptedRanges,
                                                    Option.isSome age, 
                                                    Option.isSome allowed,
                                                    Option.isSome authenticate, 
                                                    Option.isSome cacheControl, 
                                                    Option.isSome contentInfo, 
                                                    Option.isSome date, 
                                                    Option.isSome etag, 
                                                    Option.isSome expires, 
                                                    Option.isSome headers, 
                                                    Option.isSome lastModified, 
                                                    Option.isSome location, 
                                                    Option.isSome proxyAuthenticate,
                                                    Option.isSome retryAfter,
                                                    Option.isSome server,
                                                    Option.isSome vary,
                                                    Option.isSome warning) 

        member this.WithoutEntity<'TNew>(?acceptedRanges,
                                            ?age, 
                                            ?allowed,
                                            ?authenticate, 
                                            ?cacheControl, 
                                            ?contentInfo, 
                                            ?date, 
                                            ?etag, 
                                            ?expires, 
                                            ?headers, 
                                            ?lastModified, 
                                            ?location, 
                                            ?proxyAuthenticate,
                                            ?retryAfter,
                                            ?server,
                                            ?vary,
                                            ?warning) =
            HttpResponse.without this (None:Option<'TNew>) (Option.isSome acceptedRanges,
                                                            Option.isSome age, 
                                                            Option.isSome allowed,
                                                            Option.isSome authenticate, 
                                                            Option.isSome cacheControl, 
                                                            Option.isSome contentInfo, 
                                                            Option.isSome date, 
                                                            Option.isSome etag, 
                                                            Option.isSome expires, 
                                                            Option.isSome headers, 
                                                            Option.isSome lastModified, 
                                                            Option.isSome location, 
                                                            Option.isSome proxyAuthenticate,
                                                            Option.isSome retryAfter,
                                                            Option.isSome server,
                                                            Option.isSome vary,
                                                            Option.isSome warning) 

        member this.ToAsyncResponse() = async { return this }

        member this.WithoutEntityAsync<'TNew> () = async { return this.WithoutEntity<'TNew>() }

    type Status with
        member this.ToResponse<'TResp>() = HttpResponse<'TResp>.Create(this)
        member this.ToAsyncResponse<'TResp>() = async { return this.ToResponse<'TResp>() }

module HttpResponseDeserializers =
    let toAsyncMemoryStreamResponse (this:HttpResponse<Stream>) =
        let stream = this.Entity.Value
        async{
            let memStream =
                match this.ContentInfo.Length with
                | Some length -> 
                    let byteArray = Array.init<byte> (int length) (fun i -> 0uy)
                    new MemoryStream(byteArray)
                | _ -> new MemoryStream()

            let! copyResult = stream.CopyToAsync(memStream) |> Async.AwaitIAsyncResult |> Async.Catch
            return match copyResult with
                    | Choice1Of2 unit -> this.With(entity = memStream)
                    | Choice2Of2 exn -> ClientStatus.deserializationFailed.ToResponse().With(id = this.Id)
        }

    let toAsyncByteArrayResponse (this:HttpResponse<Stream>) =
        async {
            let! memResponse = toAsyncMemoryStreamResponse this
            return
                match memResponse.Entity with
                | None -> memResponse.WithoutEntity<byte[]>()
                | Some stream -> this.With(entity = stream.ToArray())    
        }

    let toAsyncStringResponse (this:HttpResponse<Stream>)  =
        let stream = this.Entity.Value
        async {
            let encoding = 
                match 
                    this.ContentInfo.MediaType 
                    |> Option.bind (fun mr -> mr.Charset) 
                    |> Option.bind (fun charset -> charset.Encoding)  with
                | Some enc -> enc
                | _ -> Encoding.UTF8

            use sr = new StreamReader(stream, encoding)

            let! result = sr.ReadToEndAsync() |> Async.AwaitTask |> Async.Catch
            return 
                match result with
                | Choice2Of2 exn ->
                    ClientStatus.deserializationFailed.ToResponse<string>().With(id = this.Id)
                | Choice1Of2 result ->
                    this.With<string>(result)
        }

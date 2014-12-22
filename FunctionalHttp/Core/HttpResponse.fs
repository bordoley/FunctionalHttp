namespace FunctionalHttp

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



[<AutoOpen>]
module HttpResponseMixins =
    type HttpResponse<'TResp> with
        member this.With<'TResp> (?acceptedRanges:AcceptableRanges,
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
            HttpResponse<'TResp>.Create(
                (if Option.isSome acceptedRanges then acceptedRanges else this.AcceptedRanges),
                (if Option.isSome age then age else this.Age),
                Set.ofSeq <| defaultArg allowed (this.Allowed :> Method seq),
                Set.ofSeq <| defaultArg authenticate (this.Authenticate :> Challenge seq),
                Set.ofSeq <| defaultArg cacheControl (this.CacheControl :> CacheDirective seq),
                defaultArg contentInfo this.ContentInfo,
                (if Option.isSome date then date else this.Date),
                this.Entity,
                (if Option.isSome etag then etag else this.ETag),
                (if Option.isSome expires then expires else this.Expires),
                defaultArg headers this.Headers,
                defaultArg id this.Id,
                (if Option.isSome lastModified then lastModified else this.LastModified),
                (if Option.isSome location then location else this.Location),
                Set.ofSeq <| defaultArg proxyAuthenticate (this.ProxyAuthenticate :> Challenge seq),
                (if Option.isSome retryAfter then retryAfter else this.RetryAfter),
                (if Option.isSome server then server else this.Server),
                defaultArg status this.Status,
                (if Option.isSome vary then vary else this.Vary),
                defaultArg version this.Version,
                List.ofSeq <| defaultArg warning (this.Warning :> Warning seq))

        member this.With<'TNew> (entity:'TNew,
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
            HttpResponse<'TNew>.Create(
                (if Option.isSome acceptedRanges then acceptedRanges else this.AcceptedRanges),
                (if Option.isSome age then age else this.Age),
                Set.ofSeq <| defaultArg allowed (this.Allowed :> Method seq),
                Set.ofSeq <| defaultArg authenticate (this.Authenticate :> Challenge seq),
                Set.ofSeq <| defaultArg cacheControl (this.CacheControl :> CacheDirective seq),
                defaultArg contentInfo this.ContentInfo,
                (if Option.isSome date then date else this.Date),
                Some entity,
                (if Option.isSome etag then etag else this.ETag),
                (if Option.isSome expires then expires else this.Expires),
                defaultArg headers this.Headers,
                defaultArg id this.Id,
                (if Option.isSome lastModified then lastModified else this.LastModified),
                (if Option.isSome location then location else this.Location),
                Set.ofSeq <| defaultArg proxyAuthenticate (this.ProxyAuthenticate :> Challenge seq),
                (if Option.isSome retryAfter then retryAfter else this.RetryAfter),
                (if Option.isSome server then server else this.Server),
                defaultArg status this.Status,
                (if Option.isSome vary then vary else this.Vary),
                defaultArg version this.Version,
                List.ofSeq <| defaultArg warning (this.Warning :> Warning seq))

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
            HttpResponse<'TResp>.Create(
                (if Option.isSome acceptedRanges then None else this.AcceptedRanges),
                (if Option.isSome age then None else this.Age),
                (if Option.isSome allowed then Set.empty else this.Allowed),
                (if Option.isSome authenticate then Set.empty else this.Authenticate),
                (if Option.isSome cacheControl then Set.empty else this.CacheControl),
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                (if Option.isSome date then None else this.Date),
                this.Entity,
                (if Option.isSome etag then None else this.ETag),
                (if Option.isSome expires then None else this.Expires),
                (if Option.isSome headers then Map.empty else this.Headers),
                this.Id,
                (if Option.isSome lastModified then None else this.LastModified),
                (if Option.isSome location then None else this.Location),
                (if Option.isSome proxyAuthenticate then Set.empty else this.ProxyAuthenticate),
                (if Option.isSome retryAfter then None else this.RetryAfter),
                (if Option.isSome server then None else this.Server),
                this.Status,
                (if Option.isSome vary then None else this.Vary),
                this.Version,
                (if Option.isSome warning then [] else this.Warning))

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
            HttpResponse<'TNew>.Create(
                (if Option.isSome acceptedRanges then None else this.AcceptedRanges),
                (if Option.isSome age then None else this.Age),
                (if Option.isSome allowed then Set.empty else this.Allowed),
                (if Option.isSome authenticate then Set.empty else this.Authenticate),
                (if Option.isSome cacheControl then Set.empty else this.CacheControl),
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                (if Option.isSome date then None else this.Date),
                None,
                (if Option.isSome etag then None else this.ETag),
                (if Option.isSome expires then None else this.Expires),
                (if Option.isSome headers then Map.empty else this.Headers),
                this.Id,
                (if Option.isSome lastModified then None else this.LastModified),
                (if Option.isSome location then None else this.Location),
                (if Option.isSome proxyAuthenticate then Set.empty else this.ProxyAuthenticate),
                (if Option.isSome retryAfter then None else this.RetryAfter),
                (if Option.isSome server then None else this.Server),
                this.Status,
                (if Option.isSome vary then None else this.Vary),
                this.Version,
                (if Option.isSome warning then [] else this.Warning))

        member this.ToAsyncResponse() = async { return this }

        member this.WithoutEntityAsync<'TNew> () = async { return this.WithoutEntity<'TNew>() }

    type Status with
        member this.ToResponse<'TResp>() = HttpResponse<'TResp>.Create(this)
        member this.ToAsyncResponse<'TResp>() = async { return this.ToResponse<'TResp>() }

module HttpStreamResponseDeserializers =
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
                    | Choice2Of2 exn -> ClientStatus.DeserializationFailed.ToResponse().With(id = this.Id)
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
                    ClientStatus.DeserializationFailed.ToResponse<string>().With(id = this.Id)
                | Choice1Of2 result ->
                    this.With<string>(result)
        }

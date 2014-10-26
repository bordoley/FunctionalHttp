namespace FunctionalHttp

open System
open System.Collections.Generic
open System.IO
open System.Text

[<Sealed>]
type HttpResponse<'TResp> internal (age:Option<TimeSpan>,
                                    allowed:Set<Method>,
                                    cacheControl: Set<CacheDirective>,
                                    contentInfo:ContentInfo,
                                    date:Option<DateTime>,
                                    entity:Option<'TResp>,
                                    etag:Option<EntityTag>,
                                    expires: Option<DateTime>,
                                    id:Guid,
                                    lastModified:Option<DateTime>,
                                    location:Option<Uri>,
                                    retryAfter:Option<DateTime>,
                                    server:Option<Server>,
                                    status:Status,
                                    version:HttpVersion) =

    static member Create<'TResp>(status, 
                                    ?age, 
                                    ?allowed, 
                                    ?cacheControl, 
                                    ?contentInfo, 
                                    ?date,
                                    ?entity:'TResp, 
                                    ?etag, 
                                    ?expires, 
                                    ?id, 
                                    ?lastModified, 
                                    ?location, 
                                    ?retryAfter,
                                    ?server,
                                    ?version) =
        HttpResponse<'TResp>(age,
            Set.ofSeq <| defaultArg allowed Seq.empty,
            Set.ofSeq <| defaultArg cacheControl Seq.empty,
            defaultArg contentInfo ContentInfo.None,
            date,
            entity,
            etag,
            expires,
            defaultArg id (Guid.NewGuid()),
            lastModified,
            location,
            retryAfter,
            server,
            status,
            defaultArg version HttpVersion.Http1_1)

    static member internal Create<'TResp>(status, entity, headers:IEnumerable<String*String>, ?id) =
        HttpResponse<'TResp>(   
            None, //age,
            Set.empty, //allowed
            Set.empty, //cacheControl,
            ContentInfo.None, //ContentInfo.Create(headers),
            None,
            Some entity, 
            None,
            None, //expires
            defaultArg id (Guid.NewGuid()),
            None,
            None, //location
            None,
            None,
            status,
            HttpVersion.Http1_1) 

    member this.Age with get() = age
    member this.Allowed with get() = allowed
    member this.CacheControl with get() = cacheControl
    member this.ContentInfo with get() = contentInfo
    member this.Date with get() = date
    member this.Entity with get() = entity
    member this.ETag with get() = etag
    member this.Expires with get() = expires
    member this.Id with get() = id
    member this.LastModified with get() = lastModified
    member this.Location with get() = location
    member this.RetryAfter with get() = retryAfter
    member this.Server with get() = server
    member this.Status with get() = status
    member this.Version with get() = version

[<AutoOpen>]
module HttpResponseMixins =
    type HttpResponse<'TResp> with
        member this.With<'TResp> (?age:TimeSpan, 
                                    ?allowed: Method seq,
                                    ?cacheControl:CacheDirective seq, 
                                    ?contentInfo:ContentInfo, 
                                    ?date:DateTime,
                                    ?etag:EntityTag,
                                    ?expires:DateTime, 
                                    ?id:Guid, 
                                    ?lastModified:DateTime,
                                    ?location:Uri, 
                                    ?retryAfter:DateTime,
                                    ?server:Server,
                                    ?status:Status,
                                    ?version:HttpVersion) =
            HttpResponse<'TResp>(
                (if Option.isSome age then age else this.Age),
                Set.ofSeq <| defaultArg allowed (this.Allowed :> Method seq),
                Set.ofSeq <| defaultArg cacheControl (this.CacheControl :> CacheDirective seq),
                defaultArg contentInfo this.ContentInfo,
                (if Option.isSome date then date else this.Date),
                this.Entity,
                (if Option.isSome etag then etag else this.ETag),
                (if Option.isSome expires then expires else this.Expires),
                defaultArg id this.Id,
                (if Option.isSome lastModified then lastModified else this.LastModified),
                (if Option.isSome location then location else this.Location),
                (if Option.isSome retryAfter then retryAfter else this.RetryAfter),
                (if Option.isSome server then server else this.Server),
                defaultArg status this.Status,
                defaultArg version this.Version)

        member this.With<'TNew> (entity:'TNew,
                                    ?age:TimeSpan, 
                                    ?allowed: Method seq,
                                    ?cacheControl:CacheDirective seq, 
                                    ?contentInfo:ContentInfo, 
                                    ?date:DateTime,
                                    ?etag:EntityTag,
                                    ?expires:DateTime, 
                                    ?id:Guid, 
                                    ?lastModified:DateTime,
                                    ?location:Uri, 
                                    ?retryAfter:DateTime,
                                    ?server:Server,
                                    ?status:Status,
                                    ?version:HttpVersion) =
            HttpResponse<'TNew>(
                (if Option.isSome age then age else this.Age),
                Set.ofSeq <| defaultArg allowed (this.Allowed :> Method seq),
                Set.ofSeq <| defaultArg cacheControl (this.CacheControl :> CacheDirective seq),
                defaultArg contentInfo this.ContentInfo,
                (if Option.isSome date then date else this.Date),
                Some entity,
                (if Option.isSome etag then etag else this.ETag),
                (if Option.isSome expires then expires else this.Expires),
                defaultArg id this.Id,
                (if Option.isSome lastModified then lastModified else this.LastModified),
                (if Option.isSome location then location else this.Location),
                (if Option.isSome retryAfter then retryAfter else this.RetryAfter),
                (if Option.isSome server then server else this.Server),
                defaultArg status this.Status,
                defaultArg version this.Version)

        member this.Without<'TResp>(?age, ?allowed, ?cacheControl, ?contentInfo, ?date, ?etag, ?expires, ?lastModified, ?location, ?retryAfter, ?server) =
            HttpResponse<'TResp>(
                (if Option.isSome age then None else this.Age),
                (if Option.isSome allowed then Set.empty else this.Allowed),
                (if Option.isSome cacheControl then Set.empty else this.CacheControl),
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                (if Option.isSome date then None else this.Date),
                this.Entity,
                (if Option.isSome etag then None else this.ETag),
                (if Option.isSome expires then None else this.Expires),
                this.Id,
                (if Option.isSome lastModified then None else this.LastModified),
                (if Option.isSome location then None else this.Location),
                (if Option.isSome retryAfter then None else this.RetryAfter),
                (if Option.isSome server then None else this.Server),
                this.Status,
                this.Version)

        member this.WithoutEntity<'TNew>(?age, ?allowed, ?cacheControl, ?contentInfo, ?date, ?etag, ?expires, ?lastModified, ?location, ?retryAfter, ?server) =
            HttpResponse<'TNew>(
                (if Option.isSome age then None else this.Age),
                (if Option.isSome allowed then Set.empty else this.Allowed),
                (if Option.isSome cacheControl then Set.empty else this.CacheControl),
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                (if Option.isSome date then None else this.Date),
                None,
                (if Option.isSome etag then None else this.ETag),
                (if Option.isSome expires then None else this.Expires),
                this.Id,
                (if Option.isSome lastModified then None else this.LastModified),
                (if Option.isSome location then None else this.Location),
                (if Option.isSome retryAfter then None else this.RetryAfter),
                (if Option.isSome server then None else this.Server),
                this.Status,
                this.Version)

        member this.ToAsyncResponse() = async { return this }

        member this.WithoutEntityAsync<'TNew> () = async { return this.WithoutEntity<'TNew>() }

    type Status with
        member this.ToResponse() = HttpResponse.Create(this)
        member this.ToAsyncResponse() = async { return this.ToResponse() }

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

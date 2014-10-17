namespace FunctionalHttp

open System
open System.Collections.Generic
open System.Linq
open System.Runtime.CompilerServices

[<Sealed>]
type HttpResponse<'TResp> internal (status:Status,
                                    entity:Option<'TResp>,
                                    id:Guid,
                                    contentInfo:ContentInfo,
                                    age:Option<TimeSpan>,
                                    cacheDirectives: Set<CacheDirective>,
                                    expires: Option<DateTimeOffset>,
                                    location:Option<Uri>)=
    static member Create<'TResp>(status, entity, ?id, ?contentInfo, ?age, ?cacheDirectives, ?expires, ?location) =
        new HttpResponse<'TResp>(
            status,
            (Some entity),
            (defaultArg id (Guid.NewGuid())),
            (defaultArg contentInfo ContentInfo.None),
            age,
            Set.ofSeq <| defaultArg cacheDirectives Seq.empty,
            expires,
            location)

    static member Create<'TResp>(status, ?id, ?contentInfo, ?age, ?cacheDirectives, ?expires, ?location) =
        new HttpResponse<'TResp>(
            status,
            None,
            (defaultArg id (Guid.NewGuid())),
            (defaultArg contentInfo ContentInfo.None),
            age,
            Set.ofSeq <| defaultArg cacheDirectives Seq.empty,
            expires,
            location)

    static member internal Create<'TResp>(status, entity, headers:IEnumerable<String*String>, ?id) =
        new HttpResponse<'TResp>(
            status, 
            Some entity, 
            (defaultArg id (Guid.NewGuid())),
            ContentInfo.None, //ContentInfo.Create(headers),
            None, //age,
            Set.empty, //cacheDirectives,
            None, //expires,
            None) //location)

    member this.Age with get() = age
    member this.CacheDirectives with get() = cacheDirectives :> seq<CacheDirective>
    member this.ContentInfo with get() = contentInfo
    member this.Entity with get() = entity
    member this.Expires with get() = expires
    member this.Id with get() = id
    member this.Location with get() = location
    member this.Status with get()= status

[<AutoOpen>]
module HttpResponseMixins =
    type HttpResponse<'TResp> with
        member this.With<'TResp> (?status, ?id, ?contentInfo, ?age, ?cacheDirectives, ?expires, ?location) =
            new HttpResponse<'TResp>(
                defaultArg status this.Status,
                this.Entity,
                defaultArg id this.Id,
                defaultArg contentInfo this.ContentInfo,
                (if Option.isSome age then age else this.Age),
                Set.ofSeq <| defaultArg cacheDirectives this.CacheDirectives,
                (if Option.isSome expires then expires else this.Expires),
                (if Option.isSome location then location else this.Location))

        member this.With<'TNew> (entity:'TNew, ?status:Status, ?id, ?contentInfo,  ?age, ?cacheDirectives, ?expires, ?location) =
            new HttpResponse<'TNew>(
                defaultArg status this.Status,
                Some entity,
                defaultArg id this.Id,
                defaultArg contentInfo this.ContentInfo,
                (if Option.isSome age then age else this.Age),
                Set.ofSeq <| defaultArg cacheDirectives this.CacheDirectives,
                (if Option.isSome expires then expires else this.Expires),
                (if Option.isSome location then location else this.Location))

        member this.Without<'TResp>(?contentInfo, ?age, ?cacheDirectives, ?expires, ?location) =
            new HttpResponse<'TResp>(
                this.Status,
                this.Entity,
                this.Id,
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                (if Option.isSome age then None else this.Age),
                Set.ofSeq <| (if Option.isSome cacheDirectives then Seq.empty else this.CacheDirectives),
                (if Option.isSome expires then None else this.Expires),
                (if Option.isSome location then None else this.Location))

        member this.WithoutEntity<'TNew>(?contentInfo, ?age, ?cacheDirectives, ?expires, ?location) =
            new HttpResponse<'TNew>(
                this.Status,
                None,
                this.Id,
                (if Option.isSome contentInfo then ContentInfo.None else this.ContentInfo),
                (if Option.isSome age then None else this.Age),
                Set.ofSeq <| (if Option.isSome cacheDirectives then Seq.empty else this.CacheDirectives),
                (if Option.isSome expires then None else this.Expires),
                (if Option.isSome location then None else this.Location))

[<AbstractClass; Sealed; Extension>]
type HttpResponseExtensions () =
    [<Extension>]
    static member ToResponse(this:Status) = HttpResponse.Create(this)

    [<Extension>]
    static member ToAsyncResponse(this:Status) = async { return HttpResponseExtensions.ToResponse(this) }

    [<Extension>]
    static member ToAsyncResponse(this:HttpResponse<'TResp>) = async { return this }

    [<Extension>]
    static member WithoutEntityAsync<'TResp> (this:HttpResponse<_>) = async { return this.WithoutEntity<'TResp>() }

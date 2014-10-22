namespace FunctionalHttp

open System

type ContentInfo =
    private {
        length:Option<int>
        location: Option<Uri>
        mediaType:Option<MediaType>
    }

    static member None = { length = None; location = None; mediaType = None }

    static member internal CreateInternal(length, location, mediaType) =
        match (length, location, mediaType)  with
        |(None, None, None) -> ContentInfo.None
        | _ -> { length = length; location = location; mediaType = mediaType }

    static member Create(?length, ?location, ?mediaType) =
        ContentInfo.CreateInternal(length, location, mediaType)

    member this.Length = this.length

    member this.Location = this.Location

    member this.MediaType = this.mediaType

module ContentInfoMixins =
    type ContentInfo with
        member this.With(?length:int, ?location:Uri, ?mediaType:MediaType) =
            ContentInfo.CreateInternal(
                (if Option.isSome length then length else this.Length),
                (if Option.isSome location then location else this.Location),
                (if Option.isSome mediaType then mediaType else this.MediaType))

        member this.Without(?length, ?location, ?mediaType) =
            ContentInfo.CreateInternal(
                (if Option.isSome length then  None else this.Length),
                (if Option.isSome location then None else this.Location),
                (if Option.isSome mediaType then None else this.MediaType))

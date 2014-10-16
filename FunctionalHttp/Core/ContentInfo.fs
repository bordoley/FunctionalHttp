namespace FunctionalHttp

open System

type ContentInfo =
    private {
        length:Option<int>
        location: Option<Uri>
        mediaRange:Option<MediaRange>
    }

    static member None = { length = None; location = None; mediaRange = None }

    static member internal CreateInternal(length, location, mediaRange) =
        match (length, location, mediaRange)  with
        |(None, None, None) -> ContentInfo.None
        | _ -> { length = length; location = location; mediaRange = mediaRange }

    static member Create(?length, ?location, ?mediaRange) =
        ContentInfo.CreateInternal(length, location, mediaRange)

    member this.Length = this.length

    member this.Location = this.Location

    member this.MediaRange = this.mediaRange

module ContentInfoExtensions =
    type ContentInfo with
        member this.With(?length:int, ?location:Uri, ?mediaRange:MediaRange) =
            ContentInfo.CreateInternal(
                (if Option.isSome length then length else this.Length),
                (if Option.isSome location then location else this.Location),
                (if Option.isSome mediaRange then mediaRange else this.MediaRange))

        member this.Without(?length, ?location, ?mediaRange) =
            ContentInfo.CreateInternal(
                (if Option.isSome length then  None else this.Length),
                (if Option.isSome location then None else this.Location),
                (if Option.isSome mediaRange then None else this.MediaRange))

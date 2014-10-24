namespace FunctionalHttp

open System

type ContentInfo =
    private {
        encodings:ContentCoding seq
        languages:LanguageTag seq
        length:Option<int>
        location: Option<Uri>
        mediaType:Option<MediaType>
    }

    static member None = { encodings = []; languages = []; length = None; location = None; mediaType = None }

    static member internal CreateInternal(encodings, languages, length, location, mediaType) =
        match (encodings, languages, length, location, mediaType)  with
        | (encodings, languages, None, None, None) when Seq.isEmpty encodings && Seq.isEmpty languages-> ContentInfo.None
        | _ -> { 
                encodings = List.ofSeq encodings; 
                languages = List.ofSeq languages; 
                length = length; 
                location = location; 
                mediaType = mediaType 
            }

    static member Create(?encodings, ?languages, ?length, ?location, ?mediaType) =
        ContentInfo.CreateInternal(defaultArg encodings [], defaultArg languages [], length, location, mediaType)

    member this.Encodings = this.encodings

    member this.Languages = this.languages

    member this.Length = this.length

    member this.Location = this.Location

    member this.MediaType = this.mediaType

module ContentInfoMixins =
    type ContentInfo with
        member this.With(?encodings, ?languages, ?length:int, ?location:Uri, ?mediaType:MediaType) =
            ContentInfo.CreateInternal(
                (if Option.isSome encodings then encodings.Value else this.Encodings),
                (if Option.isSome languages then languages.Value else this.Languages),
                (if Option.isSome length then length else this.Length),
                (if Option.isSome location then location else this.Location),
                (if Option.isSome mediaType then mediaType else this.MediaType))

        member this.Without(?encodings, ?languages, ?length, ?location, ?mediaType) =
            ContentInfo.CreateInternal(
                (if Option.isSome encodings then Seq.empty else this.Encodings),
                (if Option.isSome languages then Seq.empty else this.Languages),
                (if Option.isSome length then  None else this.Length),
                (if Option.isSome location then None else this.Location),
                (if Option.isSome mediaType then None else this.MediaType))

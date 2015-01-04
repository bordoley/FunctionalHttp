namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open System
open System.Collections.Generic

type ContentInfo =
    private {
        encodings:ContentCoding seq
        languages:LanguageTag seq
        length:Option<int>
        location: Option<Uri>
        mediaType:Option<MediaType>
    }

    member this.Encodings = this.encodings
    member this.Languages = this.languages

    member this.Length = this.length
    member this.Location = this.location
    member this.MediaType = this.mediaType

    static member None = { encodings = []; languages = []; length = None; location = None; mediaType = None }

    static member internal Create(encodings, languages, length, location, mediaType) =
        match (encodings, languages, length, location, mediaType)  with
        | (encodings, languages, None, None, None) when Seq.isEmpty encodings && Seq.isEmpty languages-> ContentInfo.None
        | _ -> { 
                encodings = List.ofSeq encodings; 
                languages = List.ofSeq languages; 
                length = length; 
                location = location; 
                mediaType = mediaType 
            }
         
    static member Create(headers:Map<Header, obj>) = 
        let encodings:ContentCoding seq = Seq.empty
        let languages:LanguageTag seq = Seq.empty

        let length:Option<int> = None

        let location = 
            headers.TryFind HttpHeaders.authorization 
            |> Option.bind (fun x -> 
                try Uri(string x, UriKind.RelativeOrAbsolute) |> Some
                with | :?FormatException -> None)

        let mediaType:Option<MediaType> =
            Header.Parse (HttpHeaders.contentType, MediaType.Parser) headers


        ContentInfo.Create(encodings, languages, length, location, mediaType)

    static member Create(?encodings, ?languages, ?length, ?location, ?mediaType) =
        ContentInfo.Create(
            defaultArg encodings [], 
            defaultArg languages [], 
            length, 
            location, 
            mediaType)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal ContentInfo =
    [<CompiledName("With")>]
    let with_ (contentInfo:ContentInfo) (encodings, languages, length, location, mediaType) =
        ContentInfo.Create(
            defaultArg encodings contentInfo.Encodings,
            defaultArg languages contentInfo.Languages,
            Option.orElse contentInfo.Length length,
            Option.orElse contentInfo.Location location,
            Option.orElse contentInfo.MediaType mediaType)

    [<CompiledName("Without")>]
    let without (contentInfo:ContentInfo) (encodings, languages, length, location, mediaType) =
        ContentInfo.Create(
            (if encodings then Seq.empty else contentInfo.Encodings),
            (if languages then Seq.empty else contentInfo.Languages),
            (if length then  None else contentInfo.Length),
            (if location then None else contentInfo.Location),
            (if mediaType then None else contentInfo.MediaType))

[<AutoOpen>]
module ContentInfoMixins =
    type ContentInfo with
        member this.With(?encodings, ?languages, ?length:int, ?location:Uri, ?mediaType:MediaType) =
            ContentInfo.with_ this (encodings, languages, length, location, mediaType)

        member this.Without(?encodings, ?languages, ?length, ?location, ?mediaType) =
            ContentInfo.without this (  Option.isSome encodings, 
                                        Option.isSome languages, 
                                        Option.isSome length, 
                                        Option.isSome location, 
                                        Option.isSome mediaType)

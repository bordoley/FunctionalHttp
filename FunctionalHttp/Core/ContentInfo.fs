namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open System
open System.Collections.Generic
open System.Globalization
open System.Text

// FIXME: ContentRange

type ContentInfo =
    private {
        encodings:ContentCoding seq
        languages:LanguageTag seq
        length:Option<uint64>
        location: Option<Uri>
        mediaType:Option<MediaType>
    }

    member this.Encodings = this.encodings
    member this.Languages = this.languages

    member this.Length = this.length
    member this.Location = this.location
    member this.MediaType = this.mediaType

    override this.ToString() =
        let builder = StringBuilder()
        let writeHeaderLine = HeaderInternal.headerLineFunc builder

        this |> ContentInfo.WriteHeaders writeHeaderLine

        string builder

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

        let length:Option<uint64> = 
            headers.TryFind HttpHeaders.contentLength
            |> Option.bind (fun x -> 
                let result = ref 0UL
                if UInt64.TryParse (string x, NumberStyles.None, NumberFormatInfo.InvariantInfo, result)
                then Some !result
                else None)

        let location = 
            HeaderInternal.parseUri HttpHeaders.contentLocation headers

        let mediaType:Option<MediaType> =
            HeaderInternal.parse (HttpHeaders.contentType, MediaType.Parser) headers

        ContentInfo.Create(encodings, languages, length, location, mediaType)

    static member Create(?encodings, ?languages, ?length, ?location, ?mediaType) =
        ContentInfo.Create(
            defaultArg encodings [], 
            defaultArg languages [], 
            length, 
            location, 
            mediaType)

    static member internal WriteHeaders (f:string*string -> unit) (contentInfo:ContentInfo)  =
        (HttpHeaders.contentEncoding, contentInfo.Encodings) |> HeaderInternal.writeSeq f 
        (HttpHeaders.contentLanguage, contentInfo.Languages) |> HeaderInternal.writeSeq f
        (HttpHeaders.contentLength,   contentInfo.Length   ) |> HeaderInternal.writeOption f
        (HttpHeaders.contentLocation, contentInfo.Location ) |> HeaderInternal.writeOption f
        (HttpHeaders.contentRange,    None                 ) |> HeaderInternal.writeOption f // FIXME
        (HttpHeaders.contentType,     contentInfo.MediaType) |> HeaderInternal.writeOption f

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal ContentInfo =
    [<CompiledName("With")>]
    let with_ (encodings, languages, length, location, mediaType) (contentInfo:ContentInfo) =
        ContentInfo.Create(
            defaultArg encodings contentInfo.Encodings,
            defaultArg languages contentInfo.Languages,
            Option.orElse contentInfo.Length length,
            Option.orElse contentInfo.Location location,
            Option.orElse contentInfo.MediaType mediaType)

    [<CompiledName("Without")>]
    let without (encodings, languages, length, location, mediaType) (contentInfo:ContentInfo) =
        ContentInfo.Create(
            (if encodings then Seq.empty else contentInfo.Encodings),
            (if languages then Seq.empty else contentInfo.Languages),
            (if length then  None else contentInfo.Length),
            (if location then None else contentInfo.Location),
            (if mediaType then None else contentInfo.MediaType))

[<AutoOpen>]
module ContentInfoMixins =
    type ContentInfo with
        member this.With(?encodings, ?languages, ?length:uint64, ?location:Uri, ?mediaType:MediaType) =
            this |> ContentInfo.with_  (encodings, languages, length, location, mediaType)

        member this.Without(?encodings, ?languages, ?length, ?location, ?mediaType) =
            this |> ContentInfo.without (   Option.isSome encodings, 
                                            Option.isSome languages, 
                                            Option.isSome length, 
                                            Option.isSome location, 
                                            Option.isSome mediaType)

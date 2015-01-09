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
        range:Option<Choice<ByteContentRange, OtherContentRange>>
    }

    member this.Encodings = this.encodings
    member this.Languages = this.languages

    member this.Length = this.length
    member this.Location = this.location
    member this.MediaType = this.mediaType
    member this.Range = this.range

    override this.ToString() =
        let builder = StringBuilder()
        let writeHeaderLine = HeaderInternal.headerLineFunc builder

        this |> ContentInfo.WriteHeaders writeHeaderLine

        string builder

    static member None = { encodings = []; languages = []; length = None; location = None; mediaType = None; range = None }

    static member internal Create(encodings, languages, length, location, mediaType, range) =
        match (encodings, languages, length, location, mediaType, range)  with
        | (encodings, languages, None, None, None, None) when Seq.isEmpty encodings && Seq.isEmpty languages-> ContentInfo.None
        | _ -> { 
                encodings = List.ofSeq encodings; 
                languages = List.ofSeq languages; 
                length = length; 
                location = location; 
                mediaType = mediaType; 
                range = range
            }
         
    static member Create(headers:Map<Header, obj>) = 
        let encodings = HeaderParsers.contentEncoding headers
        let languages:LanguageTag seq = HeaderParsers.contentLanguages headers
        let length = HeaderParsers.contentLength headers
        let location = HeaderParsers.contentLocation headers
        let mediaType = HeaderParsers.contentType headers
        let range : Option<Choice<ByteContentRange, OtherContentRange>> = None

        ContentInfo.Create(encodings, languages, length, location, mediaType, range)

    static member Create(?encodings, ?languages, ?length, ?location, ?mediaType, ?range) =
        ContentInfo.Create(
            defaultArg encodings [], 
            defaultArg languages [], 
            length, 
            location, 
            mediaType,
            range)

    static member internal WriteHeaders (f:string*string -> unit) (contentInfo:ContentInfo)  =
        (HttpHeaders.contentEncoding, contentInfo.Encodings) |> HeaderInternal.writeSeq f 
        (HttpHeaders.contentLanguage, contentInfo.Languages) |> HeaderInternal.writeSeq f
        (HttpHeaders.contentLength,   contentInfo.Length   ) |> HeaderInternal.writeOption f
        (HttpHeaders.contentLocation, contentInfo.Location ) |> HeaderInternal.writeOption f
        (HttpHeaders.contentRange,    None                 ) |> HeaderInternal.writeOption f // FIXME
        (HttpHeaders.contentType,     contentInfo.MediaType) |> HeaderInternal.writeOption f
        (HttpHeaders.contentRange,    contentInfo.Range |> function
                                        | Some (Choice1Of2 byteContentRange) -> string byteContentRange
                                        | Some (Choice2Of2 otherContentRange) -> string otherContentRange
                                        | _ -> "") |> HeaderInternal.writeObject f

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal ContentInfo =
    [<CompiledName("With")>]
    let with_ (encodings, languages, length, location, mediaType, range) (contentInfo:ContentInfo) =
        ContentInfo.Create(
            defaultArg encodings contentInfo.Encodings,
            defaultArg languages contentInfo.Languages,
            Option.orElse contentInfo.Length length,
            Option.orElse contentInfo.Location location,
            Option.orElse contentInfo.MediaType mediaType,
            Option.orElse contentInfo.Range range)

    [<CompiledName("Without")>]
    let without (encodings, languages, length, location, mediaType, range) (contentInfo:ContentInfo) =
        ContentInfo.Create(
            (if encodings then Seq.empty else contentInfo.Encodings),
            (if languages then Seq.empty else contentInfo.Languages),
            (if length then  None else contentInfo.Length),
            (if location then None else contentInfo.Location),
            (if mediaType then None else contentInfo.MediaType),
            (if range then None else contentInfo.Range))

[<AutoOpen>]
module ContentInfoMixins =
    type ContentInfo with
        member this.With(?encodings, ?languages, ?length:uint64, ?location:Uri, ?mediaType:MediaType, ?range:Choice<ByteContentRange, OtherContentRange>) =
            this |> ContentInfo.with_  (encodings, languages, length, location, mediaType, range)

        member this.Without(?encodings, ?languages, ?length, ?location, ?mediaType, ?range) =
            this |> ContentInfo.without (   Option.isSome encodings, 
                                            Option.isSome languages, 
                                            Option.isSome length, 
                                            Option.isSome location, 
                                            Option.isSome mediaType,
                                            Option.isSome range)

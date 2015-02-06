namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open System
open System.Collections.Generic
open System.Globalization
open System.Text
open System.Runtime.CompilerServices

type ContentInfo =
    private {
        encodings:ContentCoding seq
        languages:LanguageTag seq
        length:Option<uint64>
        location: Option<Uri>
        mediaType:Option<MediaType>
        range:Option<Choice<ByteContentRange, OtherContentRange>>
    }

    member this.Encodings with get() = this.encodings
    member this.Languages with get() = this.languages
    member this.Length with get() = this.length
    member this.Location with get() = this.location
    member this.MediaType with get() = this.mediaType
    member this.Range with get() = this.range

    override this.ToString() =
        let builder = StringBuilder()
        let writeHeaderLine = Header.headerLineFunc builder

        this |> ContentInfo.WriteHeaders writeHeaderLine

        string builder

    static member None = ContentInfoHelper.None

    static member internal Create(encodings, languages, length, location, mediaType, range) =
        match (encodings, languages, length, location, mediaType, range)  with
        | (encodings, languages, None, None, None, None) when Seq.isEmpty encodings && Seq.isEmpty languages -> 
            ContentInfo.None
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
        let range = HeaderParsers.contentRange headers

        ContentInfo.Create(encodings, languages, length, location, mediaType, range)

    static member Create(?encodings, ?languages, ?length, ?location, ?mediaType, ?range) =
        ContentInfo.Create(
            defaultArg encodings [], 
            defaultArg languages [], 
            length, 
            location, 
            mediaType,
            range)

    static member internal WriteHeaders (f:Header*string -> unit) (contentInfo:ContentInfo)  =
        (HttpHeaders.contentEncoding, contentInfo.Encodings) |> Header.writeSeq f 
        (HttpHeaders.contentLanguage, contentInfo.Languages) |> Header.writeSeq f
        (HttpHeaders.contentLength,   contentInfo.Length   ) |> Header.writeOption f
        (HttpHeaders.contentLocation, contentInfo.Location ) |> Header.writeOption f
        (HttpHeaders.contentRange,    None                 ) |> Header.writeOption f // FIXME
        (HttpHeaders.contentType,     contentInfo.MediaType) |> Header.writeOption f
        (HttpHeaders.contentRange,    contentInfo.Range |> function
                                        | Some (Choice1Of2 byteContentRange) -> string byteContentRange
                                        | Some (Choice2Of2 otherContentRange) -> string otherContentRange
                                        | _ -> "") |> Header.writeObject f

// F# doesn't allow for static member val variables on record types so hack around it.
and [<AbstractClass; Sealed;>] internal ContentInfoHelper () =
    static member val None = { encodings = []; languages = []; length = None; location = None; mediaType = None; range = None }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal ContentInfo =
    [<CompiledName("With")>]
    let internal with_ (encodings, languages, length, location, mediaType, range) (contentInfo:ContentInfo) =
        ContentInfo.Create(
            defaultArg encodings contentInfo.Encodings,
            defaultArg languages contentInfo.Languages,
            Option.orElse contentInfo.Length length,
            Option.orElse contentInfo.Location location,
            Option.orElse contentInfo.MediaType mediaType,
            Option.orElse contentInfo.Range range)

    [<CompiledName("Without")>]
    let internal without (encodings, languages, length, location, mediaType, range) (contentInfo:ContentInfo) =
        ContentInfo.Create(
            (if encodings then Seq.empty else contentInfo.Encodings),
            (if languages then Seq.empty else contentInfo.Languages),
            (if length then  None else contentInfo.Length),
            (if location then None else contentInfo.Location),
            (if mediaType then None else contentInfo.MediaType),
            (if range then None else contentInfo.Range))

[<AutoOpen>]
module ContentInfoExtensions =
    type ContentInfo with
        member this.With(?encodings, ?languages, ?length:uint64, ?location:Uri, ?mediaType:MediaType, ?range:Choice<ByteContentRange, OtherContentRange>) =
            this |> ContentInfo.with_  (encodings, languages, length, location, mediaType, range)

        member this.Without(?encodings:bool, ?languages:bool, ?length:bool, ?location:bool, ?mediaType:bool, ?range:bool) =
            this |> ContentInfo.without (   defaultArg encodings false, 
                                            defaultArg languages false, 
                                            defaultArg length false, 
                                            defaultArg location false, 
                                            defaultArg mediaType false,
                                            defaultArg range false)

        [<Extension>]
        member this.TryGetLength(length : byref<uint64>) = 
            Option.tryGetValue this.Length &length

        [<Extension>]
        member this.TryGetLocation(uri : byref<Uri>) = 
            Option.tryGetValue this.Location &uri

        [<Extension>]
        member this.TryGetMediaType(mediaType : byref<MediaType>) = 
            Option.tryGetValue this.MediaType &mediaType
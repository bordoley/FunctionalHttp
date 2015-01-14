namespace FunctionalHttp.Core

open System.Text

type RequestPreferences =
    private {
        acceptedCharsets: Set<Preference<Charset>>
        acceptedEncodings: Set<Preference<ContentCoding>>
        acceptedLanguages: Set<Preference<LanguageTag>>
        acceptedMediaRanges: Set<AcceptPreference>
        ranges: Option<Choice<ByteRangesSpecifier, OtherRangesSpecifier>>
    } 

    member this.AcceptedCharset = this.acceptedCharsets
    member this.AcceptedEncodings = this.acceptedEncodings
    member this.AcceptedLanguages = this.acceptedLanguages
    member this.AcceptedMediaRanges = this.acceptedMediaRanges
    member this.Ranges = this.ranges

    override this.ToString() =
        let builder = StringBuilder()
        let writeHeaderLine = HeaderInternal.headerLineFunc builder

        this |> RequestPreferences.WriteHeaders writeHeaderLine

        string builder

    static member None = { 
            acceptedCharsets = Set.empty
            acceptedEncodings = Set.empty 
            acceptedLanguages = Set.empty
            acceptedMediaRanges = Set.empty 
            ranges = None
        }

    static member internal Create (acceptedCharsets, acceptedEncodings, acceptedLanguages, acceptedMediaRanges, ranges) =
        match (acceptedCharsets, acceptedEncodings, acceptedLanguages, acceptedMediaRanges, ranges) with
        | (acceptedCharsets, acceptedEncodings, acceptedLanguages, acceptedMediaRanges, None) 
            when Seq.isEmpty acceptedCharsets && Seq.isEmpty acceptedEncodings && Seq.isEmpty acceptedLanguages && Seq.isEmpty acceptedMediaRanges ->
                RequestPreferences.None
        | _ -> 
            {
                acceptedCharsets = Set.ofSeq acceptedCharsets
                acceptedEncodings = Set.ofSeq acceptedEncodings
                acceptedLanguages = Set.ofSeq acceptedLanguages
                acceptedMediaRanges = Set.ofSeq acceptedMediaRanges
                ranges = ranges
            }

    static member Create (?acceptedCharsets, ?acceptedEncodings, ?acceptedLanguages, ?acceptedMediaRanges, ?ranges) =
        RequestPreferences.Create (
            defaultArg acceptedCharsets Seq.empty, 
            defaultArg acceptedEncodings Seq.empty, 
            defaultArg acceptedLanguages Seq.empty, 
            defaultArg acceptedMediaRanges Seq.empty, 
            ranges)

    static member Create(headers:Map<Header, obj>) = 
        let acceptedCharsets = HeaderParsers.acceptCharset headers
        let acceptedEncodings = HeaderParsers.acceptEncoding headers
        let acceptedLanguages = HeaderParsers.acceptLanguage headers
        let acceptedMediaRanges = HeaderParsers.accept headers
        let ranges = HeaderParsers.range headers

        RequestPreferences.Create (acceptedCharsets, acceptedEncodings, acceptedLanguages, acceptedMediaRanges, ranges)

    static member internal WriteHeaders (f:string*string -> unit) (preferences:RequestPreferences) = 
        (HttpHeaders.accept, preferences.AcceptedMediaRanges) |> HeaderInternal.writeSeq f
        (HttpHeaders.acceptCharset, preferences.AcceptedCharset) |> HeaderInternal.writeSeq f
        (HttpHeaders.acceptEncoding, preferences.AcceptedEncodings) |> HeaderInternal.writeSeq f
        (HttpHeaders.acceptLanguage, preferences.AcceptedLanguages) |> HeaderInternal.writeSeq f
        (HttpHeaders.range, preferences.ranges |> function
                                | Some (Choice1Of2 byteRangesSpecifier) -> string byteRangesSpecifier
                                | Some (Choice2Of2 otherRangesSpecifier) -> string otherRangesSpecifier
                                | _ -> "") |> HeaderInternal.writeObject f
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

    static member Create(headers:Map<Header, obj>) = 
        let acceptedCharsets = HeaderParsers.acceptCharset headers |> Set.ofSeq
        let acceptedEncodings = HeaderParsers.acceptEncoding headers |> Set.ofSeq
        let acceptedLanguages = HeaderParsers.acceptLanguage headers |> Set.ofSeq
        let acceptedMediaRanges = HeaderParsers.accept headers |> Set.ofSeq
        let range = HeaderParsers.range headers

        {
            acceptedCharsets = acceptedCharsets
            acceptedEncodings = acceptedEncodings
            acceptedLanguages = acceptedLanguages
            acceptedMediaRanges = acceptedMediaRanges
            ranges = range
        }

    static member internal WriteHeaders (f:string*string -> unit) (preferences:RequestPreferences) = 
        (HttpHeaders.accept, preferences.AcceptedMediaRanges) |> HeaderInternal.writeSeq f
        (HttpHeaders.acceptCharset, preferences.AcceptedCharset) |> HeaderInternal.writeSeq f
        (HttpHeaders.acceptEncoding, preferences.AcceptedEncodings) |> HeaderInternal.writeSeq f
        (HttpHeaders.acceptLanguage, preferences.AcceptedLanguages) |> HeaderInternal.writeSeq f
        (HttpHeaders.range, preferences.ranges |> function
                                | Some (Choice1Of2 byteRangesSpecifier) -> string byteRangesSpecifier
                                | Some (Choice2Of2 otherRangesSpecifier) -> string otherRangesSpecifier
                                | _ -> "") |> HeaderInternal.writeObject f
                                
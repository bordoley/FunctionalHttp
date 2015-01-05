namespace FunctionalHttp.Core

type Preference<'T> =
    private {
        range:'T
        weight:uint16
    }

type PreferenceWithParams<'T> =
    private {
        preference:Preference<'T>
        parameters:List<string*string>
    }

type RequestPreferences =
    private {
        acceptedCharsets: Set<Preference<Choice<Charset,Any>>>
        acceptedEncodings: Set<Preference<Choice<ContentCoding, Any>>>
        acceptedLanguages: Set<Preference<Choice<LanguageTag, Any>>>
        acceptedMediaRanges: Set<PreferenceWithParams<MediaRange>>
        ranges: Option<Choice<ByteRangesSpecifier, OtherRangesSpecifier>>
    } 

    member this.AcceptedCharset = this.acceptedCharsets
    member this.AcceptedEncodings = this.acceptedEncodings
    member this.AcceptedLanguages = this.acceptedLanguages
    member this.AcceptedMediaRanges = this.acceptedMediaRanges
    member this.Ranges = this.ranges

    override this.ToString() =
        // FIXME:
        ""

    static member None = { 
            acceptedCharsets = Set.empty
            acceptedEncodings = Set.empty 
            acceptedLanguages = Set.empty
            acceptedMediaRanges = Set.empty 
            ranges = None
        }

    static member Create(headers:Map<Header, obj>) = 
        // FIXME:
       RequestPreferences.None

    static member internal WriteHeaders (f:string*string -> unit) (requestPreferences:RequestPreferences) = ()
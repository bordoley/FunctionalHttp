namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open FunctionalHttp.Parsing.CharMatchers
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Core.HttpParsers

type Preference<'T> =
    private {
        range:'T
        weight:uint16
    }

    static member internal Parser (p:Parser<'T>) =()
        //let qvalue = pchar '0' .>> [ "." 0*3DIGIT ] )
        //    / ( "1" [ "." 0*3("0") ] )
        //p .>> OWS_SEMICOLON_OWS .>> pstring "q=" .>>. qvalue


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
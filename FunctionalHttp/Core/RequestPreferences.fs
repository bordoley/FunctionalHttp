namespace FunctionalHttp

type SimplePreference<'T> =
    private {
        range:'T
        score:int
    }

type PreferenceWithParams<'T when 'T : comparison> =
    private {
        range:'T
        score:int
        parameters:List<string*string>
    }

type RequestPreferences =
    private {
        acceptedCharsets: Set<SimplePreference<Charset>>
        acceptedEncodings: Set<SimplePreference<Codings>>
        acceptedLanguages: Set<SimplePreference<LanguageRange>>
        acceptedMediaRanges: Set<PreferenceWithParams<MediaRange>>
        ranges: Option<Range>
    } 

    static member None = { 
            acceptedCharsets = Set.empty
            acceptedEncodings = Set.empty 
            acceptedLanguages = Set.empty
            acceptedMediaRanges = Set.empty 
            ranges = None
        }

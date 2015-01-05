namespace FunctionalHttp.Core

type LanguageTag = 
    private
    | Language

type LanguageRange =
    private
    | LanguageTag of LanguageTag
    | Any

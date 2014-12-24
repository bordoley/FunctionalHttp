namespace FunctionalHttp.Core

type LanguageTag = 
    | Language

type LanguageRange =
    | LanguageTag of LanguageTag
    | Any

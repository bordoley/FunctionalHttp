namespace FunctionalHttp

type LanguageTag = 
    | Language

type LanguageRange =
    | LanguageTag of LanguageTag
    | Any

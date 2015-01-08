namespace FunctionalHttp.Core

open FunctionalHttp.Core.HttpParsers
open FunctionalHttp.Parsing.Parser

type LanguageTag =
    private { language: string }

    override this.ToString () = this.language

    // FIXME: Broken
    static member internal Parser =
        token |>> (fun x -> { language = x })
namespace FunctionalHttp.Core
open FunctionalHttp.Parsing
open FunctionalHttp.Core.Abnf
open FunctionalHttp.Core.HttpParsers
open FunctionalHttp.Parsing.CharParsers
open FunctionalHttp.Parsing.Parser

type LanguageTag =
    private { language: string }

    override this.ToString () = this.language

    static member internal Parser =
        let alphaKey = times 1 8 ALPHA
        let alphanumKey = pchar '-' .>>. times 1 8 ALPHA_NUMERIC |> many
        let parser = alphaKey .>>. ((pchar '-' .>>. times 1 8 ALPHA_NUMERIC) |> many)

        let doParse (stream:CharStream) =
            match parser stream with
            | Fail i -> Fail i
            | Success (_, i) -> 
                let result = { language = stream.ToString(0, i) }
                Success (result, i)
        doParse
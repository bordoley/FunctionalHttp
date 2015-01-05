namespace FunctionalHttp.Core

open FunctionalHttp.Parsing.Parser

type Any =
    private | Any

    override this.ToString() = "*"

    static member Instance = Any

    static member internal Parser =
        pstring "*" |>> fun _ -> Any
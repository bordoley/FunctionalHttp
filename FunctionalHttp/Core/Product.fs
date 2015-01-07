namespace FunctionalHttp.Core

open FunctionalHttp.Parsing

open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Parsing.CharParsers
open FunctionalHttp.Core.HttpParsers

type Product =
    private {
        name:string
        version:string
    }

    member this.Name with get() = this.name
    member this.Version with get() = this.version

    override this.ToString() =
        if this.version.Length = 0
            then this.name
        else sprintf "%s/%s" this.name this.version

    static member internal Parser =
        token .>>. opt (pchar '/' >>. token)
        |>> (function
            | (name, Some(version)) -> { name = name; version = version; }
            | (name, _) -> { name = name; version = ""; })
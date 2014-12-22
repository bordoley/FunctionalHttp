namespace FunctionalHttp

open FunctionalHttp.Parsing

open FunctionalHttp.Parsing.Parser
open FunctionalHttp.HttpParsers

type Product =
    private {
        name:string
        version:string
    }

    static member internal Parser =
        token <+> optional (parseChar '/' <+> token)
        |> map (fun result ->
            match result with
            | (name, Some(_, version)) -> { name = name; version = version; }
            | (name, _)-> { name = name; version = ""; })

    member this.Name with get() = this.name
    member this.Version with get() = this.version

    override this.ToString() =
        if this.version.Length = 0
            then this.name
        else sprintf "%s/%s" this.name this.version

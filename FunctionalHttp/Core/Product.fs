namespace FunctionalHttp.Core

open Sparse

open HttpParsers

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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Product = 
    let parser =
        token .>>. opt (pForwardSlash >>. token)
        |>> (function
            | (name, Some(version)) -> { name = name; version = version; }
            | (name, _) -> { name = name; version = ""; })
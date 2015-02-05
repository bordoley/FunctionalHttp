namespace FunctionalHttp.Core

open Sparse
open FunctionalHttp.Core.HttpParsers

type ContentCoding =
    private { contentCoding: string }

    override this.ToString () = this.contentCoding

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module ContentCoding =
    [<CompiledName("Identity")>]
    let identity = { contentCoding = "identity" }

    let internal parser =
        token |>> (fun x ->
            if x = string identity 
                then identity
            else { contentCoding = x })
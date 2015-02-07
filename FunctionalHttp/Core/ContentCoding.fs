namespace FunctionalHttp.Core

open Sparse
open FunctionalHttp.Core.HttpParsers

type ContentCoding =
    private { contentCoding: string }

    override this.ToString () = this.contentCoding

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal ContentCoding =
    let identity = { contentCoding = "identity" }

    let parser =
        token |>> (fun x ->
            if x = string identity 
                then identity
            else { contentCoding = x })

type ContentCoding with
    static member Identity = ContentCoding.identity
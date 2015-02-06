namespace FunctionalHttp.Core

open Sparse
open FunctionalHttp.Core.HttpParsers

type ContentCoding =
    private { contentCoding: string }

    override this.ToString () = this.contentCoding

    static member Identity = ContentCodingHelper.Identity

and [<AbstractClass; Sealed;>] internal ContentCodingHelper () =
    static member val Identity = { contentCoding = "identity" }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal ContentCoding =
    let internal parser =
        token |>> (fun x ->
            if x = string ContentCoding.Identity 
                then ContentCoding.Identity
            else { contentCoding = x })
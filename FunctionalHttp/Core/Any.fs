namespace FunctionalHttp.Core

open Sparse

type Any =
    private | Any

    override this.ToString() = "*"

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Any =
    let instance = Any

    let parser =
        pAsterisk |>> fun _ -> Any
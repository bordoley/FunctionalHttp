namespace FunctionalHttp.Core

open Sparse

type Any =
    private | Any

    override this.ToString() = "*"

    static member Instance = Any

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Any =
    let internal parser =
        pAsterisk |>> fun _ -> Any
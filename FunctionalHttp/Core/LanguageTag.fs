namespace FunctionalHttp.Core
open Sparse

type LanguageTag =
    private { language: string }

    override this.ToString () = this.language

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal LanguageTag =
    let parser =
        regex "[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*"
        |>> fun x -> { language = x }
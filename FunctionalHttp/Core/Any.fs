namespace FunctionalHttp.Core

open Sparse

type Any =
    private | Any

    override this.ToString() = "*"

    static member Instance = Any

    static member internal Parser =
        pAsterisk |>> fun _ -> Any
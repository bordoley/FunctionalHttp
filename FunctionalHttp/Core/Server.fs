namespace FunctionalHttp.Core

open Sparse

type Server = 
    private { userAgent : UserAgent }

    override this.ToString() = string this.userAgent

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Server = 
    let internal parser = 
        UserAgent.parser |>> (fun x -> { userAgent = x })

    [<CompiledName("Create")>]
    let create server =
        match parse parser server with
        | Success (server, _) -> server
        | _ -> invalidArg "server" "Not a valid Server string"
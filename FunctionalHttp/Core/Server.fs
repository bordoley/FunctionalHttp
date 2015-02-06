namespace FunctionalHttp.Core

open Sparse

type Server = 
    private { userAgent : UserAgent }

    override this.ToString() = string this.userAgent

    static member Create server =
        match parse ServerHelper.Parser server with
        | Success (server, _) -> server
        | _ -> invalidArg "server" "Not a valid Server string"

and [<AbstractClass; Sealed;>] internal ServerHelper () =
    static member val Parser : Parser<Server> = 
        UserAgent.parser |>> (fun x -> { userAgent = x })

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Server = 
    let internal parser = ServerHelper.Parser

namespace FunctionalHttp.Core

open Sparse

type Server = 
    private { userAgent : UserAgent }

    override this.ToString() = string this.userAgent

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Server = 
    let internal parser : Parser<Server> = 
        UserAgent.parser |>> (fun x -> { userAgent = x })

type Server with
    static member Create server =
        match parse Server.parser server with
        | Success (server, _) -> server
        | _ -> invalidArg "server" "Not a valid Server string"
namespace FunctionalHttp.Core

open FunctionalHttp.Parsing

type Server = 
    private { userAgent : UserAgent }

    override this.ToString() = string this.userAgent

    static member internal Parser = 
        UserAgent.Parser |>> (fun x -> { userAgent = x })

    static member Create server =
        match parse Server.Parser server with
        | Success (server, _) -> server
        | _ -> invalidArg "server" "Not a valid Server string"
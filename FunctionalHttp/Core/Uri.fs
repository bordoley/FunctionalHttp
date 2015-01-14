namespace FunctionalHttp.Core

open FunctionalHttp.Parsing

open UriGrammar

type DomainName =
    private { regname:string }

    override this.ToString () = this.regname

    static member internal Parser = 
        regex ( "(" + "(%[0-9A-F]{2})" + "|" + "[a-zA-Z0-9-._~!$&'()*+,;=]" + ")+" )
        |>> fun x -> { regname = x }

type HostPort =
    private { 
        host:Choice<IPv4Address, IPv6Address, DomainName> 
        port:uint16 option
    }

    override this.ToString() =
        let host =
            match this.host with
            | Choice1Of3 ip -> string ip
            | Choice2Of3 ip -> string ip
            | Choice3Of3 domain -> string domain
        let port =
            match this.port with
            | Some p -> ":" + (string p)
            | _ -> ""
        host + port

    static member internal Parser : Parser<HostPort> = pzero

(*
type Authority =
    private {
        userInfo:string
        hostPort:HostPort
    }

type Uri =
    private {
        scheme:string
        authority:Authority option
        path:List<string>
        query:string
        fragment:string
    }
*)
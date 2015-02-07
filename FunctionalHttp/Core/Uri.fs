namespace FunctionalHttp.Core

open Sparse

type DomainName =
    private { regname:string }

    override this.ToString () = this.regname

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal DomainName =    
    let parser = 
        regex "((%[0-9A-F]{2})|[a-zA-Z0-9\-\._~!\$&'\(\)*\+,;=])+"
        |>> fun x -> { regname = x }

type HostPort =
    private { 
        host:Choice<IPv4Address, IPv6Address, DomainName> 
        port:uint16 option
    }

    member this.Host with get() = this.host

    member this.Port with get() = this.port

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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal HostPort =
    let parser : Parser<HostPort> = pzero

module internal UriGrammar =
    let unreserved = "[a-zA-Z0-9\-\._~]"
    let pct_encoded = "(%[0-9A-Fa-f]{2})"
    let sub_delims = "[!\$&'\(\)\*\+,;=]"

    let pchar = "(" + unreserved + "|" + sub_delims + "|" + "[:@]" + "|" + pct_encoded + ")"

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



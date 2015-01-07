namespace FunctionalHttp.Core

open FunctionalHttp.Parsing

open FunctionalHttp.Parsing.CharMatchers
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Parsing.CharParsers
open FunctionalHttp.Core.HttpParsers

type EntityTag = 
    private {
        tag: string;
        isWeak: bool;    
    }

    member this.Tag with get() = this.tag
            
    member this.IsWeak with get() = this.isWeak

    override this.ToString() = 
        if this.isWeak 
            then sprintf "\\W\"%s\"" this.Tag
        else 
            sprintf "\"%s\"" this.Tag

    static member internal Parser =
        let etagc = is (char 0x21) <||> inRange (char 0x23) (char 0x7E) <||> HttpCharMatchers.obs_text
        let opaque_tag = (pchar '"') >>. (many1Satisfy etagc) .>> (pchar '"')
        let weak = pstring "W/"

        (opt weak) .>>. opaque_tag |>> (function
            | (Some _, tag) -> { tag = tag; isWeak = true } 
            | (_, tag) -> { tag = tag; isWeak = false })
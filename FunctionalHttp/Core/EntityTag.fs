namespace FunctionalHttp.Core

open FunctionalHttp.Parsing

open FunctionalHttp.Parsing.CharMatchers
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Core.HttpParsers

type EntityTag = 
    private {
        tag: string;
        isWeak: bool;    
    }

    static member internal Parser =
        let etagc = is (char 0x21) <||> inRange (char 0x23) (char 0x7E) <||> HttpCharMatchers.obs_text
        let opaque_tag = 
            (parseChar '"') <+> (CharMatchers.many etagc) <+> (parseChar '"')
            |> Parser.map(fun ((_, token), _) -> token)
        let weak = pstring "W/"

        (optional weak) <+> opaque_tag
        |> map (fun (w, tag) ->
            match (w, tag) with
            | (Some _, _) -> { tag = tag; isWeak = true } 
            | _ -> { tag = tag; isWeak = false })

    member this.Tag with get() = this.tag
            
    member this.IsWeak with get() = this.isWeak

    override this.ToString() = 
        if this.isWeak 
            then sprintf "\\W\"%s\"" this.Tag
        else 
            sprintf "\"%s\"" this.Tag

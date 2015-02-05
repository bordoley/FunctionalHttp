namespace FunctionalHttp.Core

open Sparse

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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal EntityTag =
    let parser =
        let etagc = regex "[\x21\x23-\x7E\x80-\xFF]+"
        let opaque_tag = pQuote >>. etagc .>> pQuote
        let weak = pstring "W/"

        (opt weak) .>>. opaque_tag |>> (function
            | (Some _, tag) -> { tag = tag; isWeak = true } 
            | (_, tag) -> { tag = tag; isWeak = false })
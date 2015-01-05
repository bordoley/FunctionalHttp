namespace FunctionalHttp.Core

open FunctionalHttp.Core.HttpParsers
open FunctionalHttp.Parsing.Parser

type ContentCoding =
    private { contentCoding: string }

    override this.ToString () = this.contentCoding

    static member Identity = { contentCoding = "identity" }

    static member internal Parser =
        token |>> (fun x ->
            if x = string ContentCoding.Identity 
                then ContentCoding.Identity
            else { contentCoding = x })
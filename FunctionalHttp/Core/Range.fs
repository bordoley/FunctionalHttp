namespace FunctionalHttp.Core

open FunctionalHttp.Parsing

open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Core.HttpParsers

type Range = private | Range


type RangeUnit = 
    private { unit: string }

    override this.ToString() = this.unit

    static member Bytes = { unit = "bytes" }

    static member internal Parser =
        token |>> fun x -> 
            if x = string RangeUnit.Bytes
            then RangeUnit.Bytes
            else { unit = x }

type AcceptsNone = 
    private | AcceptsNone

    override this.ToString() = "none"

    static member Instance = AcceptsNone

    static member internal Parser =
        pstring "none" |>> fun _ -> AcceptsNone
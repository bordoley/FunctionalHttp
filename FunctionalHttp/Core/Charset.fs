namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System
open System.Text

open FunctionalHttp.Parsing.Parser
open HttpParsers

type Charset =
    private {
        charset:string
    }

    override this.ToString() = this.charset  

    static member Any = { charset = "*" }
    static member ISO_8859_1 = { charset = "ISO-8859-1" }
    static member US_ASCII  = { charset = "US-ASCII" }
    static member UTF_16 = { charset = "UTF-16" }
    static member UTF_16BE = { charset = "UTF-16BE" }
    static member UTF_16LE = { charset = "UTF-16LE" }
    static member UTF_8 = { charset =  "UTF-8" }

    static member internal Parser : Parser<Charset> = 
        token |>> (fun parsed -> 
            match parsed.ToUpperInvariant() with
            | x when x = Charset.Any.ToString() -> Charset.Any
            | x when x = Charset.UTF_8.ToString() -> Charset.UTF_8
            | x when x = Charset.ISO_8859_1.ToString() -> Charset.ISO_8859_1
            | x when x = Charset.US_ASCII.ToString() -> Charset.US_ASCII
            | x -> { charset = x })

[<AutoOpen>]
module CharsetMixins =
    type Charset with
        member this.Encoding
            with get() =
                match this with
                | c when c = Charset.ISO_8859_1 ->
                    try
                        Some <| Encoding.GetEncoding("iso-8859-1")
                    with
                        | :? ArgumentException -> None
                | c when c = Charset.US_ASCII -> 
                    try
                        Some <| Encoding.GetEncoding("ascii")
                     with
                        | :? ArgumentException -> None
                | c when c = Charset.UTF_8 -> Some Encoding.UTF8
                | _ -> None

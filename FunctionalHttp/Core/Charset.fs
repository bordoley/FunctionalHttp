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

    static member private Charsets =
        ["ISO-8859-1"; "US-ASCII"; "UTF-16"; "UTF-16BE"; "UTF-16LE"; "UTF-8"]
        |> Seq.map (fun x -> (x, { charset = x }))
        |> Map.ofSeq

    static member Create (charset:string) =
        match charset.ToUpperInvariant() |> Charset.Charsets.TryFind  with
        | Some charset -> charset
        | None -> 
            match parse Charset.Parser charset with 
            | Some charset -> charset
            | _ -> invalidArg "charset" "not a charset"

    static member ISO_8859_1 = Charset.Create "ISO-8859-1"
    static member US_ASCII  = Charset.Create "US-ASCII"
    static member UTF_16 = Charset.Create "UTF-16" 
    static member UTF_16BE = Charset.Create "UTF-16BE" 
    static member UTF_16LE = Charset.Create "UTF-16LE" 
    static member UTF_8 = Charset.Create  "UTF-8" 

    static member internal Parser : Parser<Charset> = 
        token |>> (fun parsed -> 
            let parsed = parsed.ToUpperInvariant()
            match Charset.Charsets.TryFind parsed with
            | Some charset -> charset
            | None -> { charset = parsed })

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

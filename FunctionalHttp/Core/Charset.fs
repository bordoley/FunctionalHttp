namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse
open System
open System.Runtime.CompilerServices
open System.Text

open HttpParsers

type Charset =
    private {
        charset:string
    }

    override this.ToString() = this.charset  

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Charset = 
    let charsets : Map<string, Charset> =
        ["ISO-8859-1"; "US-ASCII"; "UTF-16"; "UTF-16BE"; "UTF-16LE"; "UTF-8"]
        |> Seq.map (fun x -> (x, { charset = x }))
        |> Map.ofSeq

    let parser : Parser<Charset> = 
        token |>> (fun parsed -> 
            let parsed = parsed.ToUpperInvariant()
            match charsets.TryFind parsed with
            | Some charset -> charset
            | None -> { charset = parsed })

    let create (charset:string) = 
        match charset.ToUpperInvariant() |> charsets.TryFind  with
        | Some charset -> charset
        | None -> 
            match parse parser charset with 
            | Success (charset, _) -> charset
            | _ -> invalidArg "charset" "not a charset"

    let iso_8859_1 = create "ISO-8859-1"

    let us_ascii = create "US-ASCII"

    let utf_16 = create "UTF-16" 

    let utf_16be = create "UTF-16BE" 

    let utf_16le = create "UTF-16LE" 

    let utf_8 = create "UTF-8"

type Charset with
    static member Create (charset:string) = Charset.create charset

    static member ISO_8859_1 = Charset.iso_8859_1

    static member US_ASCII = Charset.us_ascii

    static member UTF_16 = Charset.utf_16

    static member UTF_16BE = Charset.utf_16be

    static member UTF_16LE = Charset.utf_16le

    static member UTF_8 = Charset.utf_8

[<AutoOpen>]
module CharsetExtensions =
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

        [<Extension>]
        member this.TryGetEncoding(encoding : byref<Encoding>) = 
            Option.tryGetValue this.Encoding &encoding
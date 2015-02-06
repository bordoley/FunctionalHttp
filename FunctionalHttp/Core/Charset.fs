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

    static member Create (charset:string) = 
        match charset.ToUpperInvariant() |> CharsetHelper.Charsets.TryFind  with
        | Some charset -> charset
        | None -> 
            match parse CharsetHelper.Parser charset with 
            | Success (charset, _) -> charset

            | _ -> invalidArg "charset" "not a charset"

    static member ISO_8859_1 = CharsetHelper.ISO_8859_1

    static member US_ASCII = CharsetHelper.US_ASCII

    static member UTF_16 = CharsetHelper.UTF_16 

    static member UTF_16BE = CharsetHelper.UTF_16BE

    static member UTF_16LE = CharsetHelper.UTF_16LE

    static member UTF_8 = CharsetHelper.UTF_8 

and [<AbstractClass; Sealed;>] internal CharsetHelper () = 
    static member val Charsets : Map<string, Charset> =
        ["ISO-8859-1"; "US-ASCII"; "UTF-16"; "UTF-16BE"; "UTF-16LE"; "UTF-8"]
        |> Seq.map (fun x -> (x, { charset = x }))
        |> Map.ofSeq

    static member val Parser : Parser<Charset> = 
        token |>> (fun parsed -> 
            let parsed = parsed.ToUpperInvariant()
            match CharsetHelper.Charsets.TryFind parsed with
            | Some charset -> charset
            | None -> { charset = parsed })

    static member val ISO_8859_1 = Charset.Create "ISO-8859-1"

    static member val US_ASCII  = Charset.Create "US-ASCII"

    static member val UTF_16 = Charset.Create "UTF-16" 

    static member val UTF_16BE = Charset.Create "UTF-16BE" 

    static member val UTF_16LE = Charset.Create "UTF-16LE" 

    static member val UTF_8 = Charset.Create "UTF-8" 

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Charset = 
    let internal parser = CharsetHelper.Parser

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
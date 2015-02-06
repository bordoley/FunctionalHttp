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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Charset = 
    let internal parser = CharsetHelper.Parser

    [<CompiledName("ISO_8859_1")>]
    let iso_8859_1 = Charset.Create "ISO-8859-1"

    [<CompiledName("US_ASCII")>]
    let us_ascii  = Charset.Create "US-ASCII"

    [<CompiledName("UTF_16")>]
    let utf_16 = Charset.Create "UTF-16" 

    [<CompiledName("UTF_16BE")>]
    let utf_16be = Charset.Create "UTF-16BE" 

    [<CompiledName("UTF_16LE")>]
    let utf_16le = Charset.Create "UTF-16LE" 

    [<CompiledName("UTF_8")>]
    let utf_8 = Charset.Create "UTF-8" 

[<AutoOpen>]
module CharsetMixins =
    type Charset with
        member this.Encoding 
            with get() =
                match this with
                | c when c = Charset.iso_8859_1 ->
                    try
                        Some <| Encoding.GetEncoding("iso-8859-1")
                    with
                        | :? ArgumentException -> None
                | c when c = Charset.us_ascii -> 
                    try
                        Some <| Encoding.GetEncoding("ascii")
                     with
                        | :? ArgumentException -> None
                | c when c = Charset.utf_8 -> Some Encoding.UTF8
                | _ -> None

        [<Extension>]
        member this.TryGetEncoding(encoding : byref<Encoding>) = 
            Option.tryGetValue this.Encoding &encoding
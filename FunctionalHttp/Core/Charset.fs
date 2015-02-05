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
module Charset = 
    let private charsets =
        ["ISO-8859-1"; "US-ASCII"; "UTF-16"; "UTF-16BE"; "UTF-16LE"; "UTF-8"]
        |> Seq.map (fun x -> (x, { charset = x }))
        |> Map.ofSeq

    let internal parser : Parser<Charset> = 
        token |>> (fun parsed -> 
            let parsed = parsed.ToUpperInvariant()
            match charsets.TryFind parsed with
            | Some charset -> charset
            | None -> { charset = parsed })

    [<CompiledName("Create")>]
    let create (charset:string) =
        match charset.ToUpperInvariant() |> charsets.TryFind  with
        | Some charset -> charset
        | None -> 
            match parse parser charset with 
            | Success (charset, _) -> charset
            | _ -> invalidArg "charset" "not a charset"

    [<CompiledName("ISO_8859_1")>]
    let iso_8859_1 = create "ISO-8859-1"

    [<CompiledName("US_ASCII")>]
    let us_ascii  = create "US-ASCII"

    [<CompiledName("UTF_16")>]
    let utf_16 = create "UTF-16" 

    [<CompiledName("UTF_16BE")>]
    let utf_16be = create "UTF-16BE" 

    [<CompiledName("UTF_16LE")>]
    let utf_16le = create "UTF-16LE" 

    [<CompiledName("UTF_8")>]
    let utf_8 = create "UTF-8" 

[<AutoOpen>]
module CharsetMixins =
    type Charset with
        member this.Encoding 
            with  get() =
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
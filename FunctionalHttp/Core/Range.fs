namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System

open FunctionalHttp.Parsing.CharParsers
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Core.HttpParsers
open FunctionalHttp.Core.Abnf

open Abnf

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

type ByteRangeSpec =
    private { firstBytePos:uint64; lastBytePos:uint64 option }

    override this.ToString() =
        match (this.firstBytePos, this.lastBytePos) with
        | (fbp, Some lbp) -> (string fbp) + "-" + (string lbp)
        | _ -> (string this.firstBytePos) + "-"

    static member internal Parser =
        let digit = (many1Satisfy DIGIT) |>> UInt64.Parse
        digit.>> pchar '-' .>>. (opt digit)
        |>> fun (firstBytePos, lastBytePos) -> 
            { firstBytePos = firstBytePos; lastBytePos = lastBytePos}

type SuffixByteRangeSpec =
    private { suffixLength:uint64 }

    override this.ToString() =
        "-" + (string this.suffixLength)

    static member internal Parser =
        let digit = (many1Satisfy DIGIT) |>> UInt64.Parse
        pchar '-' >>. digit |>> fun x -> { suffixLength = x }

type ByteRangesSpecifier = 
    private { byteRangeSet: Choice<ByteRangeSpec, SuffixByteRangeSpec> Set }

    override this.ToString() =
        "bytes=" + 
        (this.byteRangeSet 
        |> Seq.map (function
            | Choice1Of2 byteRangeSpec -> string byteRangeSpec
            | Choice2Of2 suffixByteRangeSpec -> string suffixByteRangeSpec)
        |> String.concat ",")

    static member internal Parser =
        pstring "bytes=" >>. sepBy1 (ByteRangeSpec.Parser <^> SuffixByteRangeSpec.Parser) OWS_COMMA_OWS
        |>> fun x -> { byteRangeSet = Set.ofSeq x }

type OtherRangesSpecifier = 
    private { unit:string; rangeSet:string }

    override this.ToString() =
        this.unit + "=" + this.rangeSet

    static member internal Parser =
        token .>> pchar '=' .>>. (many1Satisfy VCHAR) |>> fun (k, v) -> { unit = k; rangeSet = v }
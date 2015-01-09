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
        // FIXME: UInt64.Parse can fail
        let digit = (many1Satisfy DIGIT) |>> UInt64.Parse
        digit.>> pchar '-' .>>. (opt digit)
        |>> fun (firstBytePos, lastBytePos) -> 
            { firstBytePos = firstBytePos; lastBytePos = lastBytePos}

type SuffixByteRangeSpec =
    private { suffixLength:uint64 }

    override this.ToString() =
        "-" + (string this.suffixLength)

    static member internal Parser =
        // FIXME: UInt64.Parse can fail
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
    private { unit:RangeUnit; rangeSet:string }

    override this.ToString() =
        string this.unit + "=" + this.rangeSet

    static member internal Parser =
        RangeUnit.Parser .>> pchar '=' .>>. (many1Satisfy VCHAR) |>> fun (k, v) -> { unit = k; rangeSet = v }

type ByteRangeResp =
    private { firstBytePos:uint64; lastBytePos:uint64; length:uint64 option }

    override this.ToString () =
        (string this.firstBytePos) + "-" + (string this.lastBytePos) + "/" + (match this.length with | Some l -> string l | None -> "")

    static member internal Parser =
        // FIXME: UInt64.Parse can fail
        let digit = (many1Satisfy DIGIT) |>> UInt64.Parse

        (digit .>> pchar '-') .>>. digit .>> pchar '/' .>>. (digit <^> pchar '*') |>> function 
            | ((firstBytePos, lastBytePos), Choice1Of2 length) -> 
                { firstBytePos = firstBytePos; lastBytePos = lastBytePos; length = Some length }
            | ((firstBytePos, lastBytePos), _) ->
                { firstBytePos = firstBytePos; lastBytePos = lastBytePos; length = None }

type UnsatisfiedRange =
    private { length:uint64 }

    override this.ToString () =
        "*/" + (string this.length)

    static member internal Parser =
        // FIXME: UInt64.Parse can fail
        let digit = (many1Satisfy DIGIT) |>> UInt64.Parse
        pstring "*/" >>. digit |>> fun x -> { length = x }

type ByteContentRange =
    private { range:Choice<ByteRangeResp, UnsatisfiedRange> }

    override this.ToString () =
        "bytes " + 
        match this.range with
        | Choice1Of2 byteRangeResp -> string byteRangeResp
        | Choice2Of2 unsatisfiedRange -> string unsatisfiedRange

    static member internal Parser = ByteRangeResp.Parser <^> UnsatisfiedRange.Parser |>> fun x -> { range = x }


type OtherContentRange =
    private { unit:RangeUnit; rangeResp:string }

    override this.ToString() =
        string this.unit + " " + this.rangeResp

    static member internal Parser = 
        RangeUnit.Parser .>> pchar ' ' .>>. manySatisfy CHAR
        |>> fun (unit, resp) -> { unit = unit; rangeResp = resp }
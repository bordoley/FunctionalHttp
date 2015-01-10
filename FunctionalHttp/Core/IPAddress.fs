namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System 
open Abnf

[<StructAttribute>]
type IPv4Address private (address:uint32) = 
    member this.ToUInt32 () = address

    override this.ToString() = 
        let oct0 = address &&& 0xFF000000ul >>> 24
        let oct1 = address &&& 0x00FF0000ul >>> 16
        let oct2 = address &&& 0x0000FF00ul >>> 8
        let oct3 = address &&& 0x000000FFul >>> 0
        (string oct0) + "." + (string oct1) + "." + (string oct2) + "." + (string oct3)

    static member internal Parser =
        let decOctet = 
            let pDIGIT = satisfy DIGIT
            let pReturnZero = preturn 0uy

            let toOctet x0 x1 x2 = (x0 * 100uy) + (x1 * 10uy) + x2

            let ctoi (c:char) = (byte c) - 48uy

            let followedByUpTo2DIGIT msd = 
                manyMinMaxSatisfy 0 2 DIGIT |>> fun x -> 
                    match x.Length with
                    | 0 -> msd
                    | 1 -> toOctet 0uy msd (ctoi x.[0])
                    | 2 -> toOctet msd (ctoi x.[0]) (ctoi x.[1])
                    | _ -> failwith "Invalid length should never happen"

            let followedBy0To55OrNoDigit msd =
                (pDIGIT >>= fun x ->
                    match ctoi x with
                    | x0 when x0 >= 0uy && x0 <= 4uy -> 
                        (pDIGIT |>> fun x1 -> toOctet msd x0 (ctoi x1)) <|> (toOctet 0uy msd x0 |> preturn)
                    | x0 when x0 = 5uy -> 
                        (pDIGIT >>= fun x1 ->
                            match ctoi x1 with
                            | x1 when x1 >= 0uy && x1 <= 5uy ->
                                preturn (toOctet msd x0 x1)
                            | _ -> pzero) <|> (toOctet 0uy msd x0 |> preturn)
                    | _ -> pzero) <|> (preturn msd)

            pDIGIT >>= function
                | '0' -> pReturnZero
                | '1' -> 1uy |> followedByUpTo2DIGIT
                | '2' -> followedBy0To55OrNoDigit 2uy
                |  x0  -> 
                    (pDIGIT |>> fun x1 -> toOctet 0uy (ctoi x0) (ctoi x1)) <|> preturn (ctoi x0)

        decOctet .>> pchar '.' .>>. decOctet .>> pchar '.' .>>. decOctet .>> pchar '.' .>>. decOctet 
        |>> fun (((x0, x1), x2), x3) ->
            let ip = ((uint32 x0) <<< 24) + ((uint32 x1) <<< 16) + ((uint32 x2) <<< 8) + (uint32 x3)
            IPv4Address(ip)

[<StructAttribute>]
type IPv6Address private (high:uint64, low:uint64) = 

    override this.ToString() = ""


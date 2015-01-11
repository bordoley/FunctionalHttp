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
type IPv6Address private (x0:uint32, x1:uint32, x2:uint32, x3:uint32) = 

    override this.ToString() = 
        let writeBytes x = 
            let oct0 = x &&& 0xFF000000ul >>> 24
            let oct1 = x &&& 0x00FF0000ul >>> 16
            let oct2 = x &&& 0x0000FF00ul >>> 8
            let oct3 = x &&& 0x000000FFul >>> 0

            oct0.ToString("X4") + ":" + oct1.ToString("X4") + ":" + oct2.ToString("X4") + ":" + oct3.ToString("X4")

        (writeBytes x0) + ":" + (writeBytes x1) + ":" + (writeBytes x2) + ":" + (writeBytes x3)

    static member internal Parser = 
        // What the ABNF should be
        // 0( h16 “:” ) h16  “::” [h16 | (*4(h16 “:”) ls32)]
        // 1( h16 “:” ) h16  “::” [h16 | (*3(h16 “:”) ls32)]
        // 2( h16 “:” ) h16  “::” [h16 | (*2(h16 “:”) ls32)]
        // 3( h16 “:” ) h16  “::” [h16 | (*1(h16 “:”) ls32)]
        // 4( h16 “:” ) h16  “::” [h16 | (*0(h16 “:”) ls32)]

        // 5( h16 “:” ) h16  “::” [h16]
        // 6( h16 “:” )           [(h16  “::”) | ls32]
        // “::” 5( h16 “:” ) ls32

        let h16 = manyMinMaxSatisfy 1 4 HEXDIG |>> fun x -> Convert.ToUInt16(x, 16)
        let ls32 = 
            (h16 .>> pchar ':' .>>. h16 |>> fun (x0, x1) -> 
                [|x0; x1|]) <|> 
            (IPv4Address.Parser 
            |>> fun x -> 
                let v = x.ToUInt32()
                let x0 = v &&& 0xFFFF0000ul >>> 16 |> uint16
                let x1 = v &&& 0x0000FFFFul |> uint16
                [|x0; x1|])
        
        let tail i =
            let remainder =
                (h16 |>> fun x -> [|x|])  <|>   
                ((manyMinMax 0 i (h16 .>> pchar ':')) |>> Array.ofSeq .>>. ls32
                |>> fun (x0, x1) -> 
                    Array.append x0 x1)
            
            h16 .>> pstring "::" .>>. (remainder <|>% [||])

        let _0lead = tail 4
        let _1lead = tail 3
        let _2lead = tail 2
        let _3lead = tail 1
        let _4lead = tail 0
        let _5lead = h16 .>> pstring "::" .>>. ((h16 |>> fun x -> [|x|]) <|>% [||])
        let _6lead = h16 .>> pstring "::" <^> ls32

        (((h16 .>> pchar ':' .>> notFollowedBy (pchar ':')) |> manyMinMax 0 6 |>> Array.ofSeq >>= fun x ->
            let len = Array.length x
            match len with
            | 0 -> _0lead |>> fun (head, tail) -> 
                (Array.append x [|head|], tail)
            | 1 -> _1lead |>> fun (head, tail) -> 
                (Array.append x [|head|], tail)
            | 2 -> _2lead |>> fun (head, tail) -> 
                (Array.append x [|head|], tail)
            | 3 -> _3lead |>> fun (head, tail) -> 
                (Array.append x [|head|], tail)
            | 4 -> _4lead |>> fun (head, tail) -> 
                (Array.append x [|head|], tail)
            | 5 -> _5lead |>> fun (head, tail) -> 
                (Array.append x [|head|], tail)
            | 6 -> _6lead |>> function
                                | Choice1Of2 head -> 
                                    (Array.append x [|head|], [||])
                                | Choice2Of2 tail -> (x, tail)
            | _ -> failwith "can never happen") <|>
        (pstring "::" >>. (h16 .>> pchar ':') |> manyMinMax 5 5|>> Array.ofSeq .>>. ls32)) |>> (fun (t1, t2) ->
            ([||], Array.append t1 t2)) |>> fun (x: uint16 array, y: uint16 array) ->     
                let zeros =
                    let zerosLength = (8 - x.Length - y.Length)
                    Array.create zerosLength 0us
                
                let parts = Array.concat [x; zeros; y] 
                let getUInt32 i =
                    let i = i * 2
                    let high = (uint32 parts.[i]) <<< 16
                    let low = (uint32 parts.[i+1])
                    high + low

                IPv6Address(getUInt32 0, getUInt32 1, getUInt32 2, getUInt32 3)
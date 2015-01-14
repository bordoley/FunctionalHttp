namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System 

open Abnf
open CharParsers

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
        let decOctet = regex "([2][0-4][0-9])|([2][5][0-5])|([1][0-9][0-9])|([1-9][0-9])|([0-9])"

        decOctet .>> pPeriod .>>. decOctet .>> pPeriod .>>. decOctet .>> pPeriod .>>. decOctet 
        |>> fun (((x0, x1), x2), x3) ->
            let ip = ((uint32 x0) <<< 24) + ((uint32 x1) <<< 16) + ((uint32 x2) <<< 8) + (uint32 x3)
            IPv4Address(ip)

[<StructAttribute>]
type IPv6Address private (x0:uint32, x1:uint32, x2:uint32, x3:uint32) = 

    override this.ToString() = 
        let writeBytes x = 
            let h160 = x &&& 0xFFFF0000ul >>> 16
            let h161 = x &&& 0x0000FFFFul >>> 0

            h160.ToString("X4") + ":" + h161.ToString("X4")

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

        let h16 = 
            manyMinMaxSatisfy 1 4 HEXDIG |>> fun x -> Convert.ToUInt16(x, 16)

        let ``h16 ":"`` = h16 .>> pchar ':'

        let ls32 = 
            let ipv4 = IPv4Address.Parser |>> fun x -> 
                let v = x.ToUInt32()
                let x0 = (v &&& 0xFFFF0000ul) >>> 16 |> uint16
                let x1 = (v &&& 0x0000FFFFul) |> uint16
                [|x0; x1|]
            let h16h16 = (``h16 ":"`` .>>. h16 |>> fun (x0, x1) ->  [|x0; x1|]) 
            
            ipv4 <|> h16h16
        
        let tail i =
            let remainder =
                (h16 |>> fun x -> [|x|])  <|>   
                (manyMinMax 0 i ``h16 ":"`` |>> Array.ofSeq .>>. ls32 |>> fun (x0, x1) -> Array.append x0 x1)
            
            h16 .>> pstring "::" .>>. (remainder <|>% [||])

        let _0lead = tail 4
        let _1lead = tail 3
        let _2lead = tail 2
        let _3lead = tail 1
        let _4lead = tail 0
        let _5lead = h16 .>> pstring "::" .>>. ((h16 |>> fun x -> [|x|]) <|>% [||])
        let _6lead = h16 .>> pstring "::" <^> ls32

        (h16 .>> pchar ':' .>> notFollowedBy (pchar ':') |> manyMinMax 0 6 |>> Array.ofSeq >>= fun x ->
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
            | _ -> failwith "can never happen") //<|>
        //((pstring "::" >>. (h16 .>> pchar ':') |> manyMinMax 5 5|>> Array.ofSeq .>>. ls32)) |>> (fun (t1, t2) -> ([||], Array.append t1 t2)))     
        |>> fun (x: uint16 array, y: uint16 array) ->     
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
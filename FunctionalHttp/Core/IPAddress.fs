namespace FunctionalHttp.Core

open Sparse
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
            let _250_255 = "(25[0-5])"
            let _200_249 = "(2[0-4][0-9])"
            let _100_199 = "(1[0-9][0-9])"
            let _10_99 = "([1-9][0-9])"
            let _0_9 = "([0-9])"
            regex (_250_255 + "|" + _200_249 + "|" + _100_199 + "|" + _10_99 + "|" + _0_9)

        decOctet .>> pPeriod .>>. decOctet .>> pPeriod .>>. decOctet .>> pPeriod .>>. decOctet 
        |>> fun (((x0, x1), x2), x3) ->
            let ip = ((uint32 x0) <<< 24) + ((uint32 x1) <<< 16) + ((uint32 x2) <<< 8) + (uint32 x3)
            IPv4Address(ip)

[<StructAttribute>]
type IPv6Address internal (x0:uint32, x1:uint32, x2:uint32, x3:uint32) = 

    override this.ToString() = 
        let writeBytes x = 
            let h160 = x &&& 0xFFFF0000ul >>> 16
            let h161 = x &&& 0x0000FFFFul >>> 0

            h160.ToString("X4") + ":" + h161.ToString("X4")

        (writeBytes x0) + ":" + (writeBytes x1) + ":" + (writeBytes x2) + ":" + (writeBytes x3)

    static member internal Parser =
        let h16 = manyMinMaxSatisfy 1 4 HEXDIG |>> fun x -> Convert.ToUInt16(x, 16)
        let doubleColon = pstring "::"
        let doubleOrSingleColon = doubleColon <^> pColon

        let ls32 = 
            let ipv4 = IPv4Address.Parser |>> fun x -> x.ToUInt32()
            let h16h16 = h16 .>> pColon .>>. h16 |>> fun (x0, x1) -> 
                ((uint32 x0) <<< 16) + (uint32 x1) 
            ipv4 <|> h16h16

        let parse (input:CharStream) =
            let bytes = Array.create 8 (uint16 0)

            let parseLs32 next = 
                match ls32 (input.SubStream(next)) with
                | Fail i -> Fail i
                | Success (result, next) ->
                    bytes.[6] <- uint16 (result &&& 0xFFFF0000ul >>> 16)
                    bytes.[7] <- (uint16 result)
                    Success(bytes, next)

            let rec doParse index next elided =
                let input = input.SubStream(next)

                if index < 8 then 
                    match h16 input with
                    | Fail i -> 
                        // h16 is ambiguous with ipv4 dec-octet so if parsing h16 fails
                        // here we can just return fail.
                        Fail i
                    | Success (result, next) ->
                        let input = input.SubStream(next)

                        match doubleOrSingleColon input with
                        | Success (Choice1Of2 _, next) -> 
                            if elided
                                //FIXME use better naming to avoid magic math
                                then Fail (next - 2)
                            else
                                doParse (index + 1) next true
                        | Success (Choice2Of2 _, next) -> 
                            doParse (index + 1) next elided
                        | Fail i -> 
                            // Rollback and check if at IPv4 IP4 instead
                            if (index = 5) || (elided && (index < 5)) then 
                                match IPv4Address.Parser input with
                                | Fail i -> Fail i
                                | Success (result, next) ->
                                    let result = result.ToUInt32()
                                    bytes.[index] <- uint16 (result &&& 0xFFFF0000ul >>> 16)
                                    bytes.[index + 1] <- (uint16 result)

                                    Success (index + 1, next)
                            else Fail i     
                else Success (index, next)
                     
            match doParse 0 0 false with   
            | Fail i -> Fail i 
            | Success (lastIndex, next) ->
                let offset = 8 - 1 - lastIndex
                if offset <> 0 then
                    for i = offset downto 0 do
                        bytes.[8 - 1 - i] <- bytes.[lastIndex - i]
                        bytes.[lastIndex - i] <- (uint16 0)
                
                let getUInt32 i =
                    let i = i * 2
                    let high = (uint32 bytes.[i]) <<< 16
                    let low = (uint32 bytes.[i+1])
                    high + low
                Success(IPv6Address(getUInt32 0, getUInt32 1, getUInt32 2, getUInt32 3), next)
        parse
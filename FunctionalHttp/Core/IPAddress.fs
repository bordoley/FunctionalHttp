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

    // FIXME: IPv6 grammar is very complex. Remove partial implementation for now since its incorrect.
    static member internal Parser =
        let h16 = manyMinMaxSatisfy 1 4 HEXDIG |>> fun x -> Convert.ToUInt16(x, 16)          

        let parse (input:CharStream) : ParseResult<IPv6Address> = Fail 0
        parse

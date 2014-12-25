namespace FunctionalHttp.Parsing

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts

[<Struct>]
type internal CharStream private (str: string, offset:int, length:int) =    
    new (str) = CharStream(str, 0, str.Length)

    static member val Empty = Unchecked.defaultof<CharStream>

    member this.Length with get() = length

    member this.Item(index:int) =
        if index < 0 then raise (ArgumentOutOfRangeException "index")
        if (offset + index) >= length then raise (ArgumentOutOfRangeException "index")
        Contract.EndContractBlock()

        str.Chars(offset + index)
                      
    member this.SubSequence(startIndex, length) =
        if (startIndex < 0) then raise (ArgumentOutOfRangeException "newStart")
        if (length < 0) then raise (ArgumentOutOfRangeException "newLength")
        if (this.Length < startIndex + length) then raise (ArgumentException ())
        Contract.EndContractBlock()

        match (startIndex, length) with
        | (_ ,0) -> CharStream.Empty 
        | _ when startIndex = 0 && length = this.Length -> this
        | _ -> CharStream(str, offset + startIndex, length)

    override this.ToString() = 
        match str with
        | null -> ""
        | _ -> str.Substring(offset,length)

[<AutoOpen>]
module internal CharStreamMixins =
    type CharStream with
        member this.SubSequence (start:int) =
            let computedLength = this.Length - start  
            this.SubSequence(start, computedLength)

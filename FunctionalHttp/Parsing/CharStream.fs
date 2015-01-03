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
        if index < 0 then ArgumentOutOfRangeException "index" |> raise 
        if (offset + index) >= length then ArgumentOutOfRangeException "index" |> raise
        Contract.EndContractBlock()

        str.Chars(offset + index)
                      
    member this.SubSequence(startIndex, length) =
        if (startIndex < 0) then ArgumentOutOfRangeException "newStart" |> raise
        if (length < 0) then ArgumentOutOfRangeException "newLength" |> raise
        if (this.Length < startIndex + length) then ArgumentException () |> raise
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

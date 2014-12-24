namespace FunctionalHttp.Parsing

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts

type internal CharStream private (str: string, start:int, length:int) =    
    new (str) = CharStream(str, 0, str.Length)

    static member val Empty = new CharStream("", 0, 0)

    member this.Length with get() = length

    member this.Item(index:int) =
        if (start + index) >= length then raise (ArgumentOutOfRangeException "index")
        Contract.EndContractBlock()

        str.Chars(start + index)
                      
    member this.SubSequence(newStart, newLength) =
        if (newStart < 0) then raise (ArgumentOutOfRangeException "newStart")
        if (newLength < 0) then raise (ArgumentOutOfRangeException "newLength")
        if (length < start + newStart + newLength) then raise (ArgumentException ())
        Contract.EndContractBlock()

        match (newStart, newLength) with
        | (_ ,0) -> CharStream.Empty 
        | _ when newStart = 0 && newLength = length -> this
        | _ -> CharStream(str, start + newStart, newLength)

    override this.ToString() = str.Substring(start,length)

[<AutoOpen>]
module internal CharStreamMixins =
    type CharStream with
        member this.SubSequence (start:int) =
            let computedLength = this.Length - start  
            this.SubSequence(start, computedLength)

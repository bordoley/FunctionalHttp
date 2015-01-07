namespace FunctionalHttp.Parsing

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts

type internal CharStream private (str: string, offset:int, length:int) =   
    static let empty = new CharStream("", 0, 0)

    member this.Length with get() = length

    member this.Item 
        with get(index:int) =
            if index < 0 then ArgumentOutOfRangeException "index" |> raise 
            if index >= length then ArgumentOutOfRangeException "index" |> raise
            Contract.EndContractBlock()

            str.Chars(offset + index)
                      
    member this.GetSlice(startIndex, length) =
        let startIndex = defaultArg startIndex 0
        let length = defaultArg length this.Length

        if (startIndex < 0) then ArgumentOutOfRangeException "newStart" |> raise
        if (length < 0) then ArgumentOutOfRangeException "newLength" |> raise
        if (this.Length < startIndex + length) then ArgumentException () |> raise

        match (startIndex, length) with
        | (_ ,0) -> empty
        | _ when startIndex = 0 && length = this.Length -> this
        | _ -> CharStream(str, offset + startIndex, length)

    override this.ToString() = 
        str.Substring(offset,length)

    static member Create (str:string) = 
        if str.Length = 0 then empty
        else CharStream(str, 0, str.Length)

[<AutoOpen>] 
module internal CharStreamMixins =
    type CharStream with
        member this.ToString(startIndex, length) =
            this.[startIndex..length].ToString()
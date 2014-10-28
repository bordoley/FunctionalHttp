namespace FunctionalHttp

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts

type internal CharInput private (str: string, start:int, length:int) =    
    new (str) = CharInput(str, 0, str.Length)

    static member val Empty = new CharInput("", 0, 0) :> IInput<char>

    interface IInput<char> with
        member this.Length with get() = length

        member this.Item(index:int) =
            let absoluteIndex = start + index
            let lastIndex = start + length

            Contract.Requires(absoluteIndex < lastIndex)
            str.Chars(absoluteIndex)
                          
        member this.SubSequence(newStart, newLength) =
            Contract.Requires(newStart >= 0)
            Contract.Requires(newLength >= 0)

            let oldAbsoluteLastIndex = start + length
     
            let newAbsoluteStartPos = start + newStart
            let newAbsoluteLastIndex = newAbsoluteStartPos + newLength

            Contract.Requires(newAbsoluteLastIndex <= oldAbsoluteLastIndex)

            match (newStart, newLength) with
            | (_ ,0) -> CharInput.Empty 
            | _ when newStart = 0 && newLength = length -> this :> IInput<char>
            | _ -> CharInput(str, newAbsoluteStartPos, newLength) :> IInput<char>

    override this.ToString() = str.Substring(start,length)

[<AutoOpen>]
module internal CharInputMixins =
    type String with
        member this.AsInput() = CharInput(this) :> IInput<char>
 
 module internal CharParsers =
    let parse (p:Parser<char,'TResult>) (input:String) =
        match Parser.parse p (input.AsInput()) with
        | Fail _ -> None
        | Eof _ -> None
        | Success (result,_) -> Some result     

namespace FunctionalHttp

open System
open System.Collections
open System.Collections.Generic
open System.Diagnostics.Contracts

type internal CharInput private (str: string, start:int, length:int) =    
    new (str) = CharInput(str, 0, str.Length)

    static member Empty = new CharInput("", 0, 0) :> IInput<char>

    interface IInput<char> with
        member this.Length with get() = length

        member this.Item(index:int) =
            let absoluteIndex = start + index
            let lastIndex = start + length

            Contract.Requires(absoluteIndex < lastIndex)
            str.Chars(absoluteIndex)

        member this.SubSequence newStart =
            let computedLength = length - newStart  
            (this :> IInput<char>).SubSequence(newStart, computedLength)
                          
        member this.SubSequence(newStart, newLength) =
            Contract.Requires(newStart >= 0)
            Contract.Requires(newLength >= 0)

            let oldAbsoluteLastIndex = start + length
     
            let newAbsoluteStartPos = start + newStart
            let newAbsoluteLastIndex = newAbsoluteStartPos + newLength

            Contract.Requires(newAbsoluteLastIndex <= oldAbsoluteLastIndex)

            match (newStart, newLength) with
            | (0,0) -> CharInput.Empty 
            | _ when newStart = 0 && newLength = length -> this :> IInput<char>
            | _ -> CharInput(str, newAbsoluteStartPos, length) :> IInput<char>

    override this.ToString() = str.Substring(start,length)

[<AutoOpen>]
module internal CharInputMixins =
    type String with
        member this.AsInput() = CharInput(this)
        

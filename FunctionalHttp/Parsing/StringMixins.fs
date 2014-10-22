namespace FunctionalHttp

open System
open System.Collections
open System.Collections.Generic

type private StringEnumerable(input:string) =
    static let stringEnumerator (input:string) =
        seq {
            for i = 0 to input.Length do yield input.Chars i
        }

    interface IEnumerable<char> with
        member this.GetEnumerator() = (stringEnumerator input).GetEnumerator()
        member this.GetEnumerator() = (this :> IEnumerable<char>).GetEnumerator() :> IEnumerator

[<AutoOpen>]
module internal StringMixins =
    // System.String doesn't implement IEnumerable<char> in PCL
    // see: http://stackoverflow.com/questions/11557690/why-doesnt-string-class-implement-ienumerablechar-in-portable-library
    type String with
        member this.IsEmpty = this.Length = 0
        member this.AsEnumerable() = new StringEnumerable(this) :> IEnumerable<char>


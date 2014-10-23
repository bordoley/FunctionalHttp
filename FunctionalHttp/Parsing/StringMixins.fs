namespace FunctionalHttp

open System
open System.Collections
open System.Collections.Generic

[<AutoOpen>]
module internal StringMixins =
    type String with
        member this.IsEmpty = this.Length = 0

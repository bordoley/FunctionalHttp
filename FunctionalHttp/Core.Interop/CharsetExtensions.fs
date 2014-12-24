namespace FunctionalHttp.Core.Interop

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System.Runtime.CompilerServices
open System.Text

[<AbstractClass; Sealed; Extension>]
type CharsetExtensions private () =
    [<Extension>]
    static member TryGetEncoding(this:Charset, encoding : byref<Encoding>) = 
        Option.tryGetValue this.Encoding &encoding
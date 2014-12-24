namespace FunctionalHttp.Interop

open FunctionalHttp
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type MediaTypeExtensions private () =
    [<Extension>]
    static member TryGetCharset(this:MediaType, charset : byref<Charset>) = 
        Option.tryGetValue this.Charset &charset


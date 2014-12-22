namespace FunctionalHttp.Interop

open FunctionalHttp
open System.Runtime.CompilerServices
open System.Text

[<AbstractClass; Sealed; Extension>]
type CharsetExtensions private () =
    [<Extension>]
    static member TryGetEncoding(this:Charset, encoding : byref<Encoding>) = 
        match this.Encoding with
        | None ->
            encoding <- null
            false
        | Some enc ->
            encoding <- enc
            true

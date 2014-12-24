namespace FunctionalHttp.Interop

open FunctionalHttp
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type StatusExtensions private () = 
    [<Extension>]
    static member Class(this:Status) = this.Class

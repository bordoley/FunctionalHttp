namespace FunctionalHttp.Core.Interop

open FunctionalHttp.Core
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type StatusExtensions private () = 
    [<Extension>]
    static member Class(this:Status) = this.Class

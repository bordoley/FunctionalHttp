namespace FunctionalHttp.Core.Interop

open FunctionalHttp.Core
open System.Collections.Generic
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type ChallengeExtensions private () =
    [<Extension>]
    static member TryGetData(this:Challenge, data : byref<string>) = 
        match this.DataOrParameters with
        | Choice1Of2 d ->
            data <- d
            true
        | _ ->
            data <- null
            false

    [<Extension>]
    static member TryGetParameters(this:Challenge, parameters : byref<IDictionary<string, string>>) = 
        match this.DataOrParameters with
        | Choice2Of2 p ->
            parameters <- p :> IDictionary<string, string>
            true
        | _ ->
            parameters <- null
            false

namespace FunctionalHttp.Interop

open FunctionalHttp
open System.Runtime.CompilerServices

// FIXME: Break this up into a module for F# and type extension for .Net in the interop namespace.
[<AbstractClass; Sealed; Extension>]
type SystemNetHttpClientExtensions private () = 
    [<Extension>]
    static member AsInteropHttpClient(this: System.Net.Http.HttpClient) =
        let client = this.AsFunctionalHttpClient()
        FunctionalHttp.Interop.HttpClient.FromFSharpHttpClient(client)

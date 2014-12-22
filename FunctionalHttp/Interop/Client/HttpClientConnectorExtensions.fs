﻿namespace FunctionalHttp.Interop

open FunctionalHttp
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type SystemNetHttpClientExtensions private () = 
    [<Extension>]
    static member AsInteropHttpClient(this: System.Net.Http.HttpClient) =
        let client = this.AsFunctionalHttpClient()
        FunctionalHttp.Interop.HttpClient.FromFSharpHttpClient(client)

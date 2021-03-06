﻿namespace FunctionalHttp.Client.Interop

open FunctionalHttp.Client
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type SystemNetHttpClientExtensions private () = 
    [<Extension>]
    static member AsInteropHttpClient(this: System.Net.Http.HttpClient) =
        let client = HttpClient.asFunctionalHttpClient this
        FunctionalHttp.Client.Interop.HttpClient.FromFSharpHttpClient(client)

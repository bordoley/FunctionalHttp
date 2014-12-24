﻿namespace FunctionalHttp

open System
open System.Runtime.CompilerServices

// From: https://github.com/jack-pappas/ExtCore/blob/master/ExtCore/Pervasive.fs
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Option = 
    [<CompiledName("TryGetValue")>]
    let tryGetValue (opt:Option<'T>) (result:byref<'T>) =
        match opt with
        | None -> false
        | Some retval ->
            result <- retval
            true

    [<CompiledName("OrElse")>]
    let orElse b a =
        match a with
        | Some _ -> a
        | None -> b

    [<CompiledName("OfNull")>]
    let ofNull value =
        if Object.ReferenceEquals(value, null) then None else Some value

    [<CompiledName("OfNullable")>]
    let ofNullable (value : Nullable<'T>) =
        if value.HasValue then Some value.Value else None

// From: https://github.com/fsprojects/fsharpx/blob/master/src/FSharpx.Core/CSharpCompat.fs
[<AbstractClass; Sealed; Extension>]
type internal FSharpOptionExtensions private () =
    [<Extension>]
    static member ToFSharpOption (n: Nullable<_>) =
        if n.HasValue
            then Some n.Value
            else None

    [<Extension>]
    static member ToFSharpOption v = 
        match box v with
        | null -> None
        | :? DBNull -> None
        | _ -> Some v

    [<Extension>]
    static member Select (o, f: Func<_,_>) = Option.map f.Invoke o

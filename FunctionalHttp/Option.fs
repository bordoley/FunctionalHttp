namespace FunctionalHttp
open System

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
    let inline ofNullable (value : Nullable<'T>) =
        if value.HasValue then Some value.Value else None



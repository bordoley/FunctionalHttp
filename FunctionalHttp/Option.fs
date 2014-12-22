namespace FunctionalHttp
open System

module internal Option = 
    let tryGetValue (opt:Option<'T>) (result:byref<'T>) =
        match opt with
        | None -> false
        | Some retval ->
            result <- retval
            true

    let orElse b a =
        match a with
        | Some _ -> a
        | None -> b

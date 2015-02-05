namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse

type Method =
    private {
        meth:string
    }

    override this.ToString() = this.meth

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Method = 
    let private methods =
        [   "DELETE";
            "GET";
            "HEAD";
            "OPTIONS";
            "PATCH";
            "POST";
            "PUT"]
        |> Seq.map (fun x -> (x, { meth = x }))
        |> Map.ofSeq

    let internal parser = 
        HttpParsers.token |>> (fun token -> 
            token |> methods.TryFind |> Option.getOrElseLazy (lazy { meth = token}))

    [<CompiledName("Create")>]
    let create m =
        match methods.TryFind m with
        | Some m -> m
        | _ -> 
            match parse parser m with 
            | Success (m, _) -> m
            | _ -> invalidArg "m" "not a method"

    [<CompiledName("Delete")>]
    let delete = create "DELETE"

    [<CompiledName("Get")>]
    let get = create "GET" 

    [<CompiledName("Head")>]
    let head = create "HEAD"

    [<CompiledName("Options")>]
    let options = create "OPTIONS"

    [<CompiledName("Patch")>]
    let patch = create "PATCH"

    [<CompiledName("Post")>]
    let post = create "POST"

    [<CompiledName("Put")>]
    let put = create "PUT"   
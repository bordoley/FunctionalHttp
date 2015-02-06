namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse

type Method =
    private {
        meth:string
    }

    override this.ToString() = this.meth

    static member Create m =
        match MethodHelper.Methods.TryFind m with
        | Some m -> m
        | _ -> 
            match parse MethodHelper.Parser m with 
            | Success (m, _) -> m
            | _ -> invalidArg "m" "not a method"

and [<AbstractClass; Sealed;>] internal MethodHelper () =
    static member val Methods : Map<string, Method> =
        [   "DELETE";
            "GET";
            "HEAD";
            "OPTIONS";
            "PATCH";
            "POST";
            "PUT"]
        |> Seq.map (fun x -> (x, { meth = x }))
        |> Map.ofSeq

    static member val Parser = 
        HttpParsers.token |>> (fun token -> 
            token |> MethodHelper.Methods.TryFind |> Option.getOrElseLazy (lazy { meth = token}))

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Method = 
    let internal parser = MethodHelper.Parser

    [<CompiledName("Delete")>]
    let delete = Method.Create "DELETE"

    [<CompiledName("Get")>]
    let get = Method.Create "GET" 

    [<CompiledName("Head")>]
    let head = Method.Create "HEAD"

    [<CompiledName("Options")>]
    let options = Method.Create "OPTIONS"

    [<CompiledName("Patch")>]
    let patch = Method.Create "PATCH"

    [<CompiledName("Post")>]
    let post = Method.Create "POST"

    [<CompiledName("Put")>]
    let put = Method.Create "PUT"   
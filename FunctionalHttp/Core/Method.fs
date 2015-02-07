namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse

type Method =
    private {
        meth:string
    }

    override this.ToString() = this.meth

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Method = 
    let methods : Map<string, Method> =
        [   "DELETE";
            "GET";
            "HEAD";
            "OPTIONS";
            "PATCH";
            "POST";
            "PUT"]
        |> Seq.map (fun x -> (x, { meth = x }))
        |> Map.ofSeq

    let parser = 
        HttpParsers.token |>> (fun token -> 
            token |> methods.TryFind |> Option.getOrElseLazy (lazy { meth = token}))
    
    let create m =
        match methods.TryFind m with
        | Some m -> m
        | _ -> 
            match parse parser m with 
            | Success (m, _) -> m
            | _ -> invalidArg "m" "not a method"

    let delete = create "DELETE"

    let get = create "GET" 

    let head = create "HEAD"

    let options = create "OPTIONS"

    let patch = create "PATCH"

    let post = create "POST"

    let put = create "PUT"   

type Method with
    static member Create m = Method.create m

    static member Delete = Method.delete

    static member Get = Method.get

    static member Head = Method.head

    static member Options = Method.options

    static member Patch = Method.patch

    static member Post = Method.post

    static member Put = Method.put
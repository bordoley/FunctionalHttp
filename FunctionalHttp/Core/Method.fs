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

    static member Delete = MethodHelper.Delete

    static member Get = MethodHelper.Get

    static member Head = MethodHelper.Head

    static member Options = MethodHelper.Options

    static member Patch = MethodHelper.Patch

    static member Post = MethodHelper.Post

    static member Put = MethodHelper.Put

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
    
    static member val Delete = Method.Create "DELETE"

    static member val Get = Method.Create "GET" 

    static member val Head = Method.Create "HEAD"

    static member val Options = Method.Create "OPTIONS"

    static member val Patch = Method.Create "PATCH"

    static member val Post = Method.Create "POST"

    static member val Put = Method.Create "PUT"   

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Method = 
    let internal parser = MethodHelper.Parser
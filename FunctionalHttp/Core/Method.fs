namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open FunctionalHttp.Parsing

open FunctionalHttp.Parsing.Parser

type Method =
    private {
        meth:string
    }

    override this.ToString() = this.meth

    static member private Methods =
        [   "DELETE";
            "GET";
            "HEAD";
            "OPTIONS";
            "PATCH";
            "POST";
            "PUT"]
        |> Seq.map (fun x -> (x, { meth = x }))
        |> Map.ofSeq

    static member Create m =
        match Method.Methods.TryFind m with
        | Some m -> m
        | _ -> 
            match Parser.parse Method.Parser m with 
            | Some m -> m
            | _ -> invalidArg "m" "not a method"

    static member internal Parser = 
        HttpParsers.token |>> (fun token -> 
            token |> Method.Methods.TryFind |> Option.getOrElseLazy (lazy { meth = token}))
    
    static member Delete = Method.Create "DELETE"
    static member Get = Method.Create "GET" 
    static member Head = Method.Create "HEAD"
    static member Options = Method.Create "OPTIONS"
    static member Patch = Method.Create "PATCH"
    static member Post = Method.Create "POST"
    static member Put = Method.Create "PUT"   
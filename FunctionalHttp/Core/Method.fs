namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open FunctionalHttp.Parsing

type Method =
    private {
        meth:string
    }

    static member internal Parser = 
        let methods = 
            lazy Map.ofList 
                [ ("DELETE", Method.Delete);
                  ("GET", Method.Get);
                  ("HEAD", Method.Head);
                  ("OPTIONS", Method.Options);
                  ("PATCH", Method.Patch);
                  ("POST", Method.Post);
                  ("PUT", Method.Put) ]
        HttpParsers.token 
        |> Parser.map (fun token -> 
            methods.Value.TryFind token 
            |> Option.getOrElseLazy (lazy { meth = token}))
    
    static member Delete = { meth = "DELETE" }
    static member Get = { meth = "GET" }
    static member Head = { meth = "HEAD" }
    static member Options = { meth = "OPTIONS" }
    static member Patch = { meth = "PATCH" }
    static member Post = { meth = "POST" }
    static member Put = { meth = "PUT" }

    override this.ToString() = this.meth

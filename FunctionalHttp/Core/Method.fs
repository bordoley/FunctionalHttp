namespace FunctionalHttp.Core
open FunctionalHttp.Parsing



type Method =
    private {
        meth:string
    }

    static member internal Parser = 
        HttpParsers.token |> Parser.map (fun token -> 
            match token with
            | _ when token = "GET" -> Method.Get
            | _ when token = "POST" -> Method.Post
            | _ when token = "PUT" -> Method.Put
            | _ when token = "DELETE" -> Method.Delete
            | _ -> { meth = token })
     
    static member Get = { meth = "GET" }
    static member Post = { meth = "POST" }
    static member Put = { meth = "PUT" }
    static member Delete = { meth = "DELETE" }

    override this.ToString() = this.meth

namespace FunctionalHttp.Core

type Method =
    private {
        meth:string
    }
     
    static member Get = { meth = "GET" }
    static member Post = { meth = "POST" }
    static member Put = { meth = "PUT" }
    static member Delete = { meth = "DELETE" }

    override this.ToString() = this.meth

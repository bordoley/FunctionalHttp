namespace FunctionalHttp

type Method =
    private
        | Method of meth:string
     
    static member Get = Method "GET"
    static member Post = Method "POST" 
    static member Put = Method "PUT"
    static member Delete = Method "DELETE"

    override this.ToString() = match this with Method meth -> meth

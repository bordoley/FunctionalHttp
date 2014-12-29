namespace FunctionalHttp.Server

open FunctionalHttp.Core
open System
open System.IO

type RequestFilter<'TReq> = HttpRequest<'TReq> -> HttpRequest<'TReq>
type ResponseFilter<'TResp> = HttpResponse<'TResp> -> HttpResponse<'TResp>

type IResource = 
    abstract member Route:Route with get

    abstract member Filter: HttpRequest<obj> -> HttpRequest<obj>
    abstract member Filter: HttpResponse<obj> -> HttpResponse<obj>

    abstract member Handle: HttpRequest<obj> -> Async<HttpResponse<obj>>
    abstract member Accept: HttpRequest<obj> -> Async<HttpResponse<obj>>

type IStreamResource =
    inherit IResource

    abstract member Parse: HttpRequest<Stream> -> Async<HttpRequest<obj>>
    abstract member Serialize: HttpRequest<_>*HttpResponse<obj> -> Async<HttpResponse<Stream>>
 
type IUniformResourceDelegate<'TReq> =
    abstract member RequireETagForUpdate:bool with get
    abstract member RequireIfUnmodifiedSinceForUpdate:bool with get
    abstract member Route:Route with get

    abstract member Delete: HttpRequest<_> -> Async<HttpResponse<obj>> 
    abstract member Get: HttpRequest<_> -> Async<HttpResponse<obj>> 
    abstract member Patch: HttpRequest<'TReq> -> Async<HttpResponse<obj>> 
    abstract member Post: HttpRequest<'TReq> -> Async<HttpResponse<obj>>
    abstract member Put: HttpRequest<'TReq> -> Async<HttpResponse<obj>>

    abstract member Filter: HttpRequest<obj> -> HttpRequest<obj>
    abstract member Filter: HttpResponse<obj> -> HttpResponse<obj>

type internal UniformResource<'TReq>(resource:IUniformResourceDelegate<'TReq>) =
    interface IResource with
        member this.Route with get() = resource.Route

        member this.Filter (req:HttpRequest<obj>)  = resource.Filter req

        member this.Filter (resp:HttpResponse<obj>) = resource.Filter resp

        member this.Handle req = raise (NotImplementedException())

        member this.Accept (req:HttpRequest<obj>):Async<HttpResponse<obj>> = raise (NotImplementedException())

module Resource =
    [<CompiledName("Uniform")>]
    let uniform resourceDelegate = UniformResource(resourceDelegate) :> IResource
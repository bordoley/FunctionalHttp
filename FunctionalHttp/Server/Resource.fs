namespace FunctionalHttp.Server

open FunctionalHttp.Core
open System
open System.IO

type RequestFilter<'TReq> = HttpRequest<'TReq> -> HttpRequest<'TReq>
type ResponseFilter<'TResp> = HttpResponse<'TResp> -> HttpResponse<'TResp>

type IResource = 
    abstract member Route:Route with get

    abstract member Filter: HttpRequest<'TReq> -> HttpRequest<'TReq>
    abstract member Filter: HttpResponse<'TResp> -> HttpResponse<'TResp>

    abstract member Handle: HttpRequest<_> -> Async<HttpResponse<obj>>
    abstract member Accept: HttpRequest<obj> -> Async<HttpResponse<obj>>

type IStreamResource =
    inherit IResource

    abstract member Parse: HttpRequest<Stream> -> Async<HttpRequest<obj>>
    abstract member Serialize: HttpRequest<_>*HttpResponse<obj> -> Async<HttpResponse<Stream>>
 
type IUniformResourceDelegate<'TReq> =
    abstract member RequireETagForUpdate:bool with get
    abstract member RequireIfUnmodifiedSinceForUpdate:bool with get
    abstract member Route:Route with get
    abstract member Allowed:Set<Method> with get

    abstract member Delete: HttpRequest<_> -> Async<HttpResponse<obj>> 
    abstract member Get: HttpRequest<_> -> Async<HttpResponse<obj>> 
    abstract member Patch: HttpRequest<'TReq> -> Async<HttpResponse<obj>> 
    abstract member Post: HttpRequest<'TReq> -> Async<HttpResponse<obj>>
    abstract member Put: HttpRequest<'TReq> -> Async<HttpResponse<obj>>

    abstract member Filter: HttpRequest<'TFilterReq> -> HttpRequest<'TFilterReq>
    abstract member Filter: HttpResponse<'TFilterResp> -> HttpResponse<'TFilterResp>

type internal UniformResource<'TReq>(resource:IUniformResourceDelegate<'TReq>) =
    let optionsResponse = 
        HttpResponse<obj>.Create(HttpStatus.successOk, allowed = resource.Allowed).ToAsyncResponse()

    let methodNotAllowedResponse = 
        HttpStatus.clientErrorMethodNotAllowed.ToResponse().With(allowed = resource.Allowed).ToAsyncResponse()

    let unmodified (req:HttpRequest<_>) (resp:HttpResponse<_>) = true
        //if req.preconditions.ifNoneMatch.isEmpty && req.preconditions.ifModifiedSince.isEmpty then false
        //else 
        //    let matchingTag = 
        //        resp.ETag.

    let checkUpdateConditions (req:HttpRequest<_>) = HttpStatus.successOk.ToAsyncResponse()

    let conditionalGet (req:HttpRequest<_>) = 
        async { 
           let! resp = resource.Get req
           return 
            if resp.Status.Class <> StatusClass.Success then resp
            else if (unmodified req resp) then HttpStatus.redirectionNotModified.ToResponse()
            else resp
        }

    interface IResource with
        member this.Route with get() = resource.Route

        member this.Filter (req:HttpRequest<'TFilterReq>)  = resource.Filter req

        member this.Filter (resp:HttpResponse<'TFilterResp>) = resource.Filter resp

        member this.Handle req =
            match req.Method with
            | m when not (resource.Allowed.Contains m) -> methodNotAllowedResponse
            | m when m = Method.Get || m = Method.Head -> conditionalGet req
            | m when m = Method.Post -> 
                async {
                    let! resp = resource.Get req
                    return 
                        if resp.Status.Class <> StatusClass.Success 
                        then resp
                        else HttpStatus.informationalContinue.ToResponse()
                }
            | m when m = Method.Put || m = Method.Patch ->
                async {
                    let! resp = checkUpdateConditions req
                    return 
                        if resp.Status.Class <> StatusClass.Success 
                        then resp
                        else HttpStatus.informationalContinue.ToResponse()
                }
            | m when m = Method.Delete ->
                async {
                    let! resp = resource.Get req
                    return! 
                        if resp.Status.Class <> StatusClass.Success 
                        then resp.ToAsyncResponse()
                        else resource.Delete req
                }
            | m when m = Method.Options ->  optionsResponse
            | _ -> raise (ArgumentException())

        member this.Accept (req:HttpRequest<obj>):Async<HttpResponse<obj>> = 
            match (req.Method, req.Entity) with
            | (m, Some entity) when m = Method.Post ->
                resource.Post (req.With (entity :?> 'TReq))
            | (m, Some entity) when m = Method.Put ->
                resource.Put (req.With (entity :?> 'TReq))
            | _ -> raise (ArgumentException())


module Resource =
    [<CompiledName("Uniform")>]
    let uniform resourceDelegate = UniformResource(resourceDelegate) :> IResource
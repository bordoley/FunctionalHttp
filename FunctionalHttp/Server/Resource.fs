namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System

type RequestFilter<'TReq> = HttpRequest<'TReq> -> HttpRequest<'TReq>
type ResponseFilter<'TResp> = HttpResponse<'TResp> -> HttpResponse<'TResp>

type IResource<'TReq, 'TResp> = 
    abstract member Route:Route with get

    abstract member Filter: HttpRequest<unit> -> HttpRequest<unit>
    abstract member Filter: HttpResponse<unit> -> HttpResponse<unit>

    abstract member Handle: HttpRequest<unit> -> Async<HttpResponse<Choice<'TResp, exn, unit>>>
    abstract member Accept: HttpRequest<'TReq> -> Async<HttpResponse<Choice<'TResp, exn, unit>>>

type IUniformResourceDelegate<'TReq, 'TResp> =
    abstract member RequireETagForUpdate:bool with get
    abstract member RequireIfUnmodifiedSinceForUpdate:bool with get
    abstract member Route:Route with get
    abstract member Allowed:Set<Method> with get

    abstract member Delete: HttpRequest<unit> -> Async<HttpResponse<Choice<'TResp, exn, unit>>> 
    abstract member Get: HttpRequest<unit> -> Async<HttpResponse<Choice<'TResp, exn, unit>>> 
    abstract member Patch: HttpRequest<'TReq> -> Async<HttpResponse<Choice<'TResp, exn, unit>>> 
    abstract member Post: HttpRequest<'TReq> -> Async<HttpResponse<Choice<'TResp, exn, unit>>>
    abstract member Put: HttpRequest<'TReq> -> Async<HttpResponse<Choice<'TResp, exn, unit>>>

module Resource =
    [<CompiledName("Uniform")>]
    let uniform (resource: IUniformResourceDelegate<'TReq,'TResp>) = 
        let optionsResponse = 
            HttpResponse<Choice<'TResp, exn, unit>>.Create(HttpStatus.successOk, Choice3Of3 (), allowed = resource.Allowed) |> Async.result

        let methodNotAllowedResponse = 
            HttpResponse<Choice<'TResp, exn, unit>>.Create(HttpStatus.clientErrorMethodNotAllowed, Choice3Of3 (), allowed = resource.Allowed) |> Async.result

        let continueResponse = HttpResponse<Choice<'TResp, exn, unit>>.Create(HttpStatus.informationalContinue, Choice3Of3 ())

        let continueIfSuccess (resp:HttpResponse<Choice<'TResp, exn, unit>>) =
            if resp.Status.Class <> StatusClass.Success then resp else continueResponse
           
        let unmodified (req:HttpRequest<unit>) (resp:HttpResponse<Choice<'TResp, exn, unit>>) = true
            //if req.preconditions.ifNoneMatch.isEmpty && req.preconditions.ifModifiedSince.isEmpty then false
            //else 
            //    let matchingTag = 
            //        resp.ETag.

        let checkUpdateConditions (req:HttpRequest<unit>) = 
            HttpResponse<Choice<'TResp, exn, unit>>.Create(HttpStatus.successOk, Choice3Of3 ()) |> Async.result

        let conditionalGet (req:HttpRequest<unit>) = 
            async { 
               let! resp = resource.Get req
               return 
                if resp.Status.Class <> StatusClass.Success then resp
                else if (unmodified req resp) 
                    then 
                        HttpResponse<Choice<'TResp, exn, unit>>.Create(HttpStatus.redirectionNotModified, Choice3Of3 (), allowed = resource.Allowed)
                else resp
            }

        { new IResource<'TReq, 'TResp> with
            member this.Route with get() = resource.Route

            member this.Filter (req:HttpRequest<unit>)  = req

            member this.Filter (resp:HttpResponse<unit>) = resp

            member this.Handle req =
                match req.Method with
                | m when not (resource.Allowed.Contains m) -> methodNotAllowedResponse
                | m when m = Method.Get || m = Method.Head -> conditionalGet req
                | m when m = Method.Post -> 
                    async {
                        let! resp = resource.Get req
                        return continueIfSuccess resp
                    }
                | m when m = Method.Put || m = Method.Patch ->
                    async {
                        let! resp = checkUpdateConditions req
                        return continueIfSuccess resp
                    }
                | m when m = Method.Delete ->
                    async {
                        let! resp = resource.Get req
                        return! 
                            if resp.Status.Class <> StatusClass.Success 
                            then resp |> Async.result
                            else resource.Delete req
                    }
                | m when m = Method.Options ->  optionsResponse
                | _ -> raise (ArgumentException())

            member this.Accept req = 
                match req.Method with
                | m when m = Method.Post ->
                    resource.Post req
                | _ -> raise (ArgumentException())
    }

    [<CompiledName("Authorizing")>]
    let authorizing (authorizers: seq<string*IAuthorizer>, resource:IResource<'TReq,'TResp>) =
        let authorizers = authorizers |> Map.ofSeq

        let unauthorizedResponse = 
            let challenges = authorizers |> Map.toSeq |> Seq.map (fun (k,v) -> v.AuthenticationChallenge)
            HttpResponse<Choice<'TResp, exn, unit>>.Create(HttpStatus.clientErrorUnauthorized, Choice3Of3 (), authenticate = challenges) |> Async.result

        let forbiddenResponse = 
            HttpResponse<Choice<'TResp, exn, unit>>.Create(HttpStatus.clientErrorForbidden , Choice3Of3 ()) |> Async.result

        { new IResource<'TReq,'TResp> with
            member this.Route with get() = resource.Route

            member this.Filter (req:HttpRequest<unit>)  = resource.Filter req

            member this.Filter (resp:HttpResponse<unit>) = resource.Filter resp

            member this.Handle (req:HttpRequest<unit>) =
                req.Authorization
                |> Option.bind (fun a -> authorizers.TryFind a.Scheme)
                |> Option.map (fun authorizer -> 
                    async {
                        let! authenticated  = authorizer.Authenticate req
                        return!
                            if authenticated 
                            then resource.Handle req
                            else forbiddenResponse
                    })
                |> Option.getOrElse unauthorizedResponse

            member this.Accept req = resource.Accept req
        } 

    [<CompiledName("WithFilters")>]
    let withFilters (requestFilter:RequestFilter<unit>, responseFilter:ResponseFilter<unit>) (resource:IResource<'TReq, 'TResp>) =
        { new IResource<'TReq, 'TResp> with
            member this.Route with get() = resource.Route

            member this.Filter req = requestFilter req

            member this.Filter resp = responseFilter resp

            member this.Handle req = resource.Handle req
        
            member this.Accept req = resource.Accept req
        }
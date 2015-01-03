namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System

type RequestFilter<'TReq> = HttpRequest<'TReq> -> HttpRequest<'TReq>
type ResponseFilter<'TResp> = HttpResponse<'TResp> -> HttpResponse<'TResp>

type IResource<'TReq, 'TResp> = 
    abstract member Route:Route with get

    abstract member FilterRequest: HttpRequest<'TFilterReq> -> HttpRequest<'TFilterReq>
    abstract member FilterResponse: HttpResponse<'TFilterResp> -> HttpResponse<'TFilterResp>

    abstract member Handle: HttpRequest<unit> -> Async<HttpResponse<Option<'TResp>>>
    abstract member Accept: HttpRequest<'TReq> -> Async<HttpResponse<Option<'TResp>>>

type IUniformResourceDelegate<'TReq, 'TResp> =
    abstract member RequireETagForUpdate:bool with get
    abstract member RequireIfUnmodifiedSinceForUpdate:bool with get
    abstract member Route:Route with get
    abstract member Allowed:Set<Method> with get

    abstract member Delete: HttpRequest<unit> -> Async<HttpResponse<unit>> 
    abstract member Get: HttpRequest<unit> -> Async<HttpResponse<Option<'TResp>>> 
    abstract member Patch: HttpRequest<'TReq> -> Async<HttpResponse<Option<'TResp>>> 
    abstract member Post: HttpRequest<'TReq> -> Async<HttpResponse<Option<'TResp>>>
    abstract member Put: HttpRequest<'TReq> -> Async<HttpResponse<Option<'TResp>>>

module Resource =
    [<CompiledName("Create")>]
    let create (route, handle, accept) =
        { new IResource<'TReq, 'TReqp> with
            member this.Route = route

            member this.FilterRequest (req: HttpRequest<_>) = req

            member this.FilterResponse (resp: HttpResponse<_>) = resp

            member this.Handle (req:HttpRequest<unit>) = handle req

            member this.Accept (req: HttpRequest<'TReq>) = accept req
        }

    [<CompiledName("Uniform")>]
    let uniform (resource: IUniformResourceDelegate<'TReq,'TResp>) = 
        let optionsResponse = 
            HttpResponse<Option<'TResp>>.Create(HttpStatus.successOk, None, allowed = resource.Allowed) |> async.Return

        let methodNotAllowedResponse = 
            HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorMethodNotAllowed, None, allowed = resource.Allowed) |> async.Return

        let continueResponse = HttpResponse<Option<'TResp>>.Create(HttpStatus.informationalContinue, None)

        let continueIfSuccess (resp:HttpResponse<Option<'TResp>>) =
            if resp.Status.Class <> StatusClass.Success then resp else continueResponse
           
        let unmodified (req:HttpRequest<unit>, resp:HttpResponse<Option<'TResp>>) = true
            //if req.preconditions.ifNoneMatch.isEmpty && req.preconditions.ifModifiedSince.isEmpty then false
            //else 
            //    let matchingTag = 
            //        resp.ETag.

        let checkUpdateConditions (req:HttpRequest<unit>) = 
            HttpResponse<Option<'TResp>>.Create(HttpStatus.successOk, None) |> async.Return

        let conditionalGet (req:HttpRequest<unit>) = 
            async { 
                let! resp = resource.Get req
                return 
                    if resp.Status.Class <> StatusClass.Success then resp
                    else if unmodified (req, resp) then HttpResponse<Option<'TResp>>.Create(HttpStatus.redirectionNotModified, None, allowed = resource.Allowed)
                    else resp
            }

        { new IResource<'TReq, 'TResp> with
            member this.Route with get() = resource.Route

            member this.FilterRequest (req:HttpRequest<_>)  = req

            member this.FilterResponse (resp:HttpResponse<_>) = resp

            member this.Handle req =
                match req.Method with
                | m when not (resource.Allowed.Contains m) -> methodNotAllowedResponse
                | m when m = Method.Get || m = Method.Head -> conditionalGet req
                | m when m = Method.Post -> 
                    req |> resource.Get |> Async.map continueIfSuccess
                | m when m = Method.Put || m = Method.Patch -> 
                    req |> checkUpdateConditions |> Async.map continueIfSuccess
                | m when m = Method.Delete ->
                    async {
                        let! resp = resource.Get req
                        return! 
                            if resp.Status.Class <> StatusClass.Success 
                            then resp |> async.Return
                            else req |> resource.Delete |> Async.map (HttpResponse.withEntity None)
                    }
                | m when m = Method.Options ->  optionsResponse
                | _ -> ArgumentException() |> raise

            member this.Accept req = 
                match req.Method with
                | m when m = Method.Post ->
                    resource.Post req
                | _ -> ArgumentException() |> raise
    }

    [<CompiledName("Authorizing")>]
    let authorizing (authorizers: seq<string*IAuthorizer>, resource:IResource<'TReq,'TResp>) =
        let authorizers = authorizers |> Map.ofSeq

        let unauthorizedResponse = 
            let challenges = authorizers |> Map.toSeq |> Seq.map (fun (k,v) -> v.AuthenticationChallenge)
            HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorUnauthorized, None, authenticate = challenges) |> async.Return

        let forbiddenResponse = 
            HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorForbidden , None) |> async.Return

        { new IResource<'TReq,'TResp> with
            member this.Route with get() = resource.Route

            member this.FilterRequest (req:HttpRequest<_>)  = resource.FilterRequest req

            member this.FilterResponse (resp:HttpResponse<_>) = resource.FilterResponse resp

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
    let withFilters ( requestFilter: RequestFilter<unit>, responseFilter: ResponseFilter<unit>) (resource:IResource<'TReq, 'TResp>) =
        { new IResource<'TReq, 'TResp> with
            member this.Route with get() = resource.Route

            member this.FilterRequest req = req.With(()) |> requestFilter |> (fun x -> x.With(req.Entity))

            member this.FilterResponse resp = resp.With(()) |> responseFilter |> (fun x -> x.With(resp.Entity))

            member this.Handle req = resource.Handle req
        
            member this.Accept req = resource.Accept req
        }

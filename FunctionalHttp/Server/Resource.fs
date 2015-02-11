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

type UniformResourceBuilder<'TReq, 'TResp> () =
    let requireETagForUpdate = ref false
    let requireIfUnmodifiedSinceForUpdate = ref false
    let route = ref None

    let delete : Option<HttpRequest<unit> -> Async<HttpResponse<unit>>> ref            = ref None
    let get :    Option<HttpRequest<unit> -> Async<HttpResponse<Option<'TResp>>>> ref  = ref None
    let head :   Option<HttpRequest<unit> -> Async<HttpResponse<unit>>> ref            = ref None
    let patch :  Option<HttpRequest<'TReq> -> Async<HttpResponse<Option<'TResp>>>> ref = ref None
    let post :   Option<HttpRequest<'TReq> -> Async<HttpResponse<Option<'TResp>>>> ref = ref None
    let put :    Option<HttpRequest<'TReq> -> Async<HttpResponse<Option<'TResp>>>> ref = ref None

    let requestFilter  = ref (fun (req:HttpRequest<unit>) -> req)
    let responseFilter = ref (fun (req:HttpResponse<unit>) -> req)

    member this.RequireETagForUpdate 
        with set value = 
            requireETagForUpdate := value

    member this.RequireIfUnmodifiedSinceForUpdate
        with set value = 
            requireIfUnmodifiedSinceForUpdate := value

    member this.Route
        with set value =
            route := Some value

    member this.Delete
        with set value =
            delete := Some value

    member this.Get
        with set value =
            get := Some value

    member this.Head
        with set value =
            head := Some value
        
    member this.Patch
        with set value =
            patch := Some value

    member this.Post
        with set value =
            post := Some value

    member this.Put
        with set value =
            put := Some value

    member this.RequestFilter
        with set value =
            requestFilter := value

    member this.ResponseFilter
        with set value =
            responseFilter := value

    member this.Build () =
        let requireETagForUpdate = !requireETagForUpdate
        let requireIfUnmodifiedSinceForUpdate = !requireIfUnmodifiedSinceForUpdate

        // Builder must set route
        let route = (!route).Value

        let delete = !delete

        // Builder must set GET method handler
        let get = (!get).Value
        let head = 
            match (!head, get) with
            | (Some head, _) -> Some head
            | (_, get) -> 
                let handle req =
                    async {
                        let! resp = get req
                        return resp.With(())
                    }
                Some handle

        let patch = !patch
        let post = !post
        let put = !put

        let requestFilter  = !requestFilter
        let responseFilter = !responseFilter 

        let allowedMethods = 
            [ 
                delete |> Option.map (fun _ -> Method.Delete);
                patch  |> Option.map (fun _ -> Method.Patch);
                post   |> Option.map (fun _ -> Method.Post);
                put    |> Option.map (fun _ -> Method.Put);

                Some Method.Get;
                Some Method.Head;
                Some Method.Options;
            ] |> Seq.choose (fun x -> x) |> Set.ofSeq

        let optionsResponse = 
            HttpResponse.create HttpStatus.successOk None |> HttpResponse.withAllowed allowedMethods |> async.Return
        
        let methodNotAllowedResponse = 
            HttpResponse.create HttpStatus.clientErrorMethodNotAllowed None |> HttpResponse.withAllowed allowedMethods |> async.Return

        let continueResponse = 
            HttpResponse.create HttpStatus.informationalContinue None

        let redirectionNotModifiedResponse =
            HttpResponse.create HttpStatus.redirectionNotModified None |> HttpResponse.withAllowed allowedMethods

        let continueIfSuccess (resp:HttpResponse<Option<'TResp>>) =
            if resp.Status.Class <> StatusClass.Success then resp else continueResponse
             
        let unmodified (req:HttpRequest<unit>, resp:HttpResponse<Option<'TResp>>) = false
            //if req.preconditions.ifNoneMatch.isEmpty && req.preconditions.ifModifiedSince.isEmpty then false
            //else 
            //    let matchingTag = 
            //        resp.ETag.

        let checkUpdateConditions (req:HttpRequest<unit>) = 
            HttpResponse.create HttpStatus.successOk None |> async.Return

        let conditionalGet (req:HttpRequest<unit>) = 
            async { 
                let! resp = get req
                return 
                    if resp.Status.Class <> StatusClass.Success then resp
                    else if unmodified (req, resp) then redirectionNotModifiedResponse
                    else resp
            }
   
        { new IResource<'TReq, 'TResp> with
            member this.Route = route

            member this.FilterRequest req = req.With(()) |> requestFilter |> (fun x -> x.With(req.Entity))

            member this.FilterResponse resp = resp.With(()) |> responseFilter |> (fun x -> x.With(resp.Entity))

            member this.Handle (req:HttpRequest<unit>) = 
                match req.Method with
                | m when not (allowedMethods.Contains m) -> methodNotAllowedResponse
                | m when m = Method.Get || m = Method.Head -> conditionalGet req
                | m when m = Method.Post -> 
                    req |> get |> Async.map continueIfSuccess
                | m when m = Method.Put || m = Method.Patch -> 
                    req |> checkUpdateConditions |> Async.map continueIfSuccess
                | m when m = Method.Delete ->
                    async {
                        let! resp = get req
                        return! 
                            if resp.Status.Class <> StatusClass.Success 
                            then resp |> async.Return
                            else req |> delete.Value |> Async.map (HttpResponse.withEntity None)
                    }
                | m when m = Method.Options ->  optionsResponse
                | _ -> ArgumentException() |> raise

            member this.Accept (req: HttpRequest<'TReq>) = 
                match req.Method with
                | m when m = Method.Post -> post.Value req
                | m when m = Method.Put -> put.Value req
                | m when m = Method.Patch -> patch.Value req
                | _ -> ArgumentException() |> raise
        }

module Resource =
    [<CompiledName("Create")>]
    let create (route, handle, accept) =
        { new IResource<'TReq, 'TResp> with
            member this.Route = route

            member this.FilterRequest (req: HttpRequest<_>) = req

            member this.FilterResponse (resp: HttpResponse<_>) = resp

            member this.Handle (req:HttpRequest<unit>) = handle req

            member this.Accept (req: HttpRequest<'TReq>) = accept req
        }

    [<CompiledName("Authorizing")>]
    let authorizing (authorizers: seq<IAuthorizer>) (resource:IResource<'TReq,'TResp>) =
        let authorizers = authorizers |> Seq.map (fun x -> (x.Scheme, x)) |> Map.ofSeq

        let unauthorizedResponse = 
            let challenges = authorizers |> Map.toSeq |> Seq.map (fun (k,v) -> v.AuthenticationChallenge)
            HttpResponse.create HttpStatus.clientErrorUnauthorized None |> HttpResponse.withAuthenticationChallenges challenges |> async.Return

        let forbiddenResponse = 
            HttpResponse.create HttpStatus.clientErrorForbidden None |> async.Return

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
    let withFilters (requestFilter: RequestFilter<unit>, responseFilter: ResponseFilter<unit>) (resource:IResource<'TReq, 'TResp>) =
        { new IResource<'TReq, 'TResp> with
            member this.Route with get() = resource.Route

            member this.FilterRequest req = req.With(()) |> requestFilter |> (fun x -> x.With(req.Entity))

            member this.FilterResponse resp = resp.With(()) |> responseFilter |> (fun x -> x.With(resp.Entity))

            member this.Handle req = resource.Handle req
        
            member this.Accept req = resource.Accept req
        }

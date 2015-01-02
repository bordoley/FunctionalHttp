namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open FunctionalHttp.Parsing
open System
open System.IO
open System.Text

type RequestFilter<'TReq> = HttpRequest<'TReq> -> HttpRequest<'TReq>
type ResponseFilter<'TResp> = HttpResponse<'TResp> -> HttpResponse<'TResp>

type IResource = 
    abstract member Route:Route with get

    abstract member Filter: HttpRequest<unit> -> HttpRequest<unit>
    abstract member Filter: HttpResponse<obj> -> HttpResponse<obj>

    abstract member Handle: HttpRequest<unit> -> Async<HttpResponse<obj>>
    abstract member Accept: HttpRequest<obj> -> Async<HttpResponse<obj>>

type IStreamResource =
    inherit IResource

    abstract member Parse: HttpRequest<Stream> -> Async<HttpRequest<Choice<obj,exn>>>
    abstract member Serialize: HttpRequest<_>*HttpResponse<obj> -> Async<HttpResponse<Stream>>
 
type IUniformResourceDelegate<'TReq> =
    abstract member RequireETagForUpdate:bool with get
    abstract member RequireIfUnmodifiedSinceForUpdate:bool with get
    abstract member Route:Route with get
    abstract member Allowed:Set<Method> with get

    abstract member Delete: HttpRequest<unit> -> Async<HttpResponse<obj>> 
    abstract member Get: HttpRequest<unit> -> Async<HttpResponse<obj>> 
    abstract member Patch: HttpRequest<'TReq> -> Async<HttpResponse<obj>> 
    abstract member Post: HttpRequest<'TReq> -> Async<HttpResponse<obj>>
    abstract member Put: HttpRequest<'TReq> -> Async<HttpResponse<obj>>

type IAuthorizer =
  abstract member AuthenticationChallenge : Challenge with get
  abstract member Scheme:string with get
  abstract member Authenticate: HttpRequest<unit> -> Async<bool>

module internal ResourceInternal =
    let cast (resp : HttpResponse<_>) =
        resp.With(resp.Entity :> obj)

module Authorizer =
    [<CompiledName("Basic")>]
    let basic (realm:string) (f:HttpRequest<unit>*string*string -> Async<bool>) =
        let challengeString = sprintf "basic realm=\"%s\", encoding=\"UTF-8\"" realm
        let challenge = challengeString |> Parser.parse Challenge.Parser |> Option.get

        { new IAuthorizer with
            member this.AuthenticationChallenge with get () = challenge

            member this.Scheme = "Basic"

            member this.Authenticate (req:HttpRequest<unit>) =
                async {
                    let creds = req.Authorization.Value
                    return! 
                        match creds.DataOrParameters with
                        | Choice1Of2 base64Data ->
                            let bytes = Convert.FromBase64String base64Data
                            let decodedString = Encoding.UTF8.GetString(bytes, 0, bytes.Length)
                            let userPwd = decodedString.Split([|':'|], 2, StringSplitOptions.None)
                            if userPwd.Length <> 2
                            then Async.result false
                            else f(req, userPwd.[0], userPwd.[1])
                        | _ -> async{ return false }
                }
        }

module Resource =
    [<CompiledName("Uniform")>]
    let uniform (resource: IUniformResourceDelegate<'TReq>) = 
        let optionsResponse = 
            HttpResponse<obj>.Create(HttpStatus.successOk, () :> obj, allowed = resource.Allowed) |> Async.result

        let methodNotAllowedResponse = 
            HttpResponse<obj>.Create(HttpStatus.clientErrorMethodNotAllowed, () :> obj, allowed = resource.Allowed) |> Async.result

        let continueResponse = HttpResponse<obj>.Create(HttpStatus.informationalContinue, () :> obj)
        let continueIfSuccess (resp:HttpResponse<obj>) =
            if resp.Status.Class <> StatusClass.Success then resp else continueResponse

        let unmodified (req:HttpRequest<_>) (resp:HttpResponse<_>) = true
            //if req.preconditions.ifNoneMatch.isEmpty && req.preconditions.ifModifiedSince.isEmpty then false
            //else 
            //    let matchingTag = 
            //        resp.ETag.

        let checkUpdateConditions (req:HttpRequest<_>) = 
            HttpStatus.successOk
            |> Status.toResponse
            |> ResourceInternal.cast
            |> Async.result

        let conditionalGet (req:HttpRequest<_>) = 
            async { 
               let! resp = resource.Get req
               return 
                if resp.Status.Class <> StatusClass.Success then resp
                else if (unmodified req resp) 
                    then 
                        HttpStatus.redirectionNotModified
                        |> Status.toResponse
                        |> fun x -> x.With(allowed = resource.Allowed)
                        |> ResourceInternal.cast
                else resp
            }

        { new IResource with
            member this.Route with get() = resource.Route

            member this.Filter (req:HttpRequest<unit>)  = req

            member this.Filter (resp:HttpResponse<obj>) = resp

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

            member this.Accept (req:HttpRequest<obj>):Async<HttpResponse<obj>> = 
                match (req.Method, req.Entity) with
                | (m, entity) when m = Method.Post && (entity :? 'TReq)->
                    let typedReq = req.With(entity :?> 'TReq)
                    resource.Post typedReq
                | _ -> raise (ArgumentException())
    }

    [<CompiledName("Authorizing")>]
    let authorizing (authorizers: seq<string*IAuthorizer>, resource:IResource) =
        let authorizers = authorizers |> Map.ofSeq

        let unauthorizedResponse = 
            let challenges = authorizers |> Map.toSeq |> Seq.map (fun (k,v) -> v.AuthenticationChallenge)
            HttpStatus.clientErrorUnauthorized
            |> Status.toResponse
            |> fun x -> x.With(authenticate = challenges)
            |> ResourceInternal.cast
            |> Async.result

        let forbiddenResponse = 
            HttpStatus.clientErrorForbidden 
            |> Status.toResponse 
            |> ResourceInternal.cast
            |> Async.result

        { new IResource with
            member this.Route with get() = resource.Route

            member this.Filter (req:HttpRequest<unit>)  = resource.Filter req

            member this.Filter (resp:HttpResponse<obj>) = resource.Filter resp

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
    let withFilters (requestFilter:RequestFilter<unit>, responseFilter:ResponseFilter<obj>) (resource:IResource) =
        { new IResource with
            member this.Route with get() = resource.Route

            member this.Filter req = requestFilter req

            member this.Filter resp = responseFilter resp

            member this.Handle req = resource.Handle req
        
            member this.Accept req = resource.Accept req
        }
namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System.IO

type IServerResource =
    abstract member Route:Route with get

    abstract member FilterRequest: HttpRequest<Stream> -> HttpRequest<Stream>
    abstract member FilterResponse: HttpResponse<Stream> -> HttpResponse<Stream>

    abstract member Handle: HttpRequest<Stream> -> Async<HttpResponse<Stream>>
    abstract member Accept: HttpRequest<Stream> -> Async<HttpResponse<Stream>>

module ServerResource =
    [<CompiledName("Create")>]
    let create (parse:HttpRequest<Stream> -> Async<HttpRequest<Choice<'TReq,exn>>>, serialize) (resource:IResource<'TReq,'TResp>) = 
        let badRequestResponse = HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorBadRequest, None) |> Async.result

        {new IServerResource with
            member this.Route = resource.Route

            member this.FilterRequest (req: HttpRequest<Stream>) = req.With(()) |> resource.FilterRequest |> HttpRequest.withEntity req.Entity
    
            member this.FilterResponse (resp: HttpResponse<Stream>) = resp.With(()) |> resource.FilterResponse |> HttpResponse.withEntity resp.Entity

            member this.Handle req = 
                async {
                    let reqWithoutEntity = req.With(())
                    let! resp = resource.Handle reqWithoutEntity
                    return! serialize(reqWithoutEntity, resp)
                }

            member this.Accept req = 
                async {
                    let! req = parse req
                    let! resp = 
                        match req.Entity with
                        | Choice1Of2 entity -> resource.Accept (req.With(entity))
                        | Choice2Of2 ex -> badRequestResponse

                    return! serialize(req.With(()), resp)
                }
        }
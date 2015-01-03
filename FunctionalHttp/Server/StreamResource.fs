namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System
open System.IO

type RequestParser<'TReq> = HttpRequest<Stream> -> Async<HttpRequest<Choice<'TReq,exn>>>
type ResponseSerializer<'TResp> = HttpRequest<unit>*HttpResponse<Option<'TResp>> -> Async<HttpResponse<Stream>>

type IStreamResource =
    abstract member Route:Route with get

    abstract member FilterRequest: HttpRequest<Stream> -> HttpRequest<Stream>
    abstract member FilterResponse: HttpResponse<Stream> -> HttpResponse<Stream>

    abstract member Handle: HttpRequest<Stream> -> Async<HttpResponse<Stream>>
    abstract member Accept: HttpRequest<Stream> -> Async<HttpResponse<Stream>>

module StreamResource =
    [<CompiledName("Create")>]
    let create (parse:RequestParser<'TReq>, serialize:ResponseSerializer<'TResp>) (resource:IResource<'TReq,'TResp>) = 
        let badRequestResponse = HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorBadRequest, None) |> async.Return

        {new IStreamResource with
            member this.Route = resource.Route

            member this.FilterRequest (req: HttpRequest<Stream>) = resource.FilterRequest req
    
            member this.FilterResponse (resp: HttpResponse<Stream>) = resource.FilterResponse resp

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

        (*
    let contentType (parsers: seq<MediaType*RequestParser<'TReq>>, serializers: seq<MediaType*ResponseSerializer<'TResp>> ) (resource:IResource<'TReq,'TResp>) =
        let parsers = Map.ofSeq parsers
        let serializers = Map.ofSeq serializers

        let parse (req:HttpRequest<Stream>) = 
            match req.ContentInfo.MediaType with
            | Some mediaType -> 
                let parser = parsers.Item mediaType
                parser req
            | _ -> InvalidOperationException() |> raise

        let serialize (req:HttpRequest<unit>, resp:HttpResponse<Option<'TResp>>) =
            // FIXME
            resp.With(Stream.Null) |> Async.result

        let delegateServerResource = create (parse, serialize) resource

        {new IStreamResource with
            member this.Route = delegateServerResource.Route

            member this.FilterRequest (req: HttpRequest<Stream>) = delegateServerResource.FilterRequest req
    
            member this.FilterResponse (resp: HttpResponse<Stream>) = delegateServerResource.FilterResponse resp

            member this.Handle req = 
                async {
                    let! resp = delegateServerResource.Handle req

                }

            member this.Accept req = 
                async {

                }
        }*)
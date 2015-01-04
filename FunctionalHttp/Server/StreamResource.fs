namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System
open System.IO

type RequestParser<'TReq> = HttpRequest<Stream> -> Async<HttpRequest<Choice<'TReq,exn>>>
type ResponseSerializer<'TResp> = HttpRequest<unit>*HttpResponse<Option<'TResp>> -> Async<HttpResponse<Stream>>

type IStreamResource =
    abstract member Route:Route with get

    abstract member Process: HttpRequest<Stream> -> Async<HttpResponse<Stream>>

module StreamResource =
    [<CompiledName("Create")>]
    let create (parse:RequestParser<'TReq>, serialize:ResponseSerializer<'TResp>) (resource:IResource<'TReq,'TResp>) = 
        let badRequestResponse = HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorBadRequest, None) |> async.Return

        {new IStreamResource with
            member this.Route = resource.Route

            member this.Process req = 
                async {
                    let req =  resource.FilterRequest req

                    let reqWithoutEntity = req.With(())
                    let! resp = resource.Handle reqWithoutEntity

                    let! resp = 
                        if resp.Status <> HttpStatus.informationalContinue
                        then resp |> async.Return
                        else 
                            async {
                                let! req = parse req
                                return! 
                                    match req.Entity with
                                    | Choice1Of2 entity -> req.With(entity) |> resource.Accept 
                                    | Choice2Of2 ex -> badRequestResponse
                            }

                    return! serialize(reqWithoutEntity, resp) |> Async.map resource.FilterResponse
            }
        }

    [<CompiledName("ByteRange")>]
    let byteRange (resource:IStreamResource) =
        { new IStreamResource with
            member this.Route = resource.Route

            member this.Process req = 
                async {
                    let! resp = resource.Process req
                    return! 
                        match (resp.Status, req.Preferences.Ranges) with
                        | (status, Some range) when status = HttpStatus.successOk ->
                            // FIXME:
                            async.Return resp                    
                        | _ -> async.Return resp
                }
        }

    [<CompiledName("ContentTypeNegotiating")>]
    let contentTypeNegotiating (parsers: seq<MediaType*RequestParser<'TReq>>, serializers: seq<MediaType*ResponseSerializer<'TResp>> ) (resource:IResource<'TReq,'TResp>) =
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
            resp.With(Stream.Null) |> async.Return

        let connegResource = 
            { new IResource<'TReq,'TResp> with 
                member this.Route with get() = resource.Route

                member this.FilterRequest req = resource.FilterRequest req
                member this.FilterResponse resp = resource.FilterResponse resp

                member this.Handle req = 
                    //FIXME:
                    resource.Handle req

                member this.Accept resp = 
                    // FIXME:
                    resource.Accept resp
            }

        create (parse, serialize) connegResource

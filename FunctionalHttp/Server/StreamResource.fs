namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open System
open System.IO
open FunctionalHttp.Core

type ResponseConverterProvider<'TResp> = HttpRequest<unit> -> Converter<'TResp,Stream>

type IStreamResource =
    abstract member Route:Route with get

    abstract member Process: HttpRequest<Stream> -> Async<HttpResponse<Stream>>

module StreamResource =
    [<CompiledName("Create")>]
    let create (parser:Converter<Stream,'TReq>, serializer:ResponseConverterProvider<'TResp>) (resource:IResource<'TReq,'TResp>) = 

        // Fixme: The only reason with response entity is an option is the bad request handler. I feel like there is a better way,
        // since ideally servers could return different entity type for different responses as either Choice or a DU type 'TResp
        let badRequestResponse = HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorBadRequest, None) |> async.Return

        let parse = parser |> HttpRequest.convert

        let serialize (req, resp:HttpResponse<Option<'TResp>>) = 
            let converter = serializer req
            match resp.Entity with
            | Some str -> resp.With(str)|> HttpResponse.convertOrThrow converter 
            | None -> resp.With(Stream.Null) |> async.Return

        {new IStreamResource with
            member this.Route = resource.Route

            member this.Process req = async {
                let req = resource.FilterRequest req

                let reqWithoutEntity = req.With(())
                let! resp = resource.Handle reqWithoutEntity

                let! resp = 
                    if resp.Status <> HttpStatus.informationalContinue
                    then resp |> async.Return
                    else async {
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
        let acceptedRanges = Choice1Of2 (Set.empty.Add RangeUnit.Bytes)

        { new IStreamResource with
            member this.Route = resource.Route

            member this.Process req = async {
                let! resp = resource.Process req
                return 
                    match (resp.Status, req.Preferences.Ranges) with
                    | (status, Some (Choice1Of2 range)) when status = HttpStatus.successOk && resp.Entity.CanSeek ->
                        match range.ByteRangeSet with
                        | Choice1Of2 byteRangeSpec::[] -> 
                            let firstBytePos = Math.Min(byteRangeSpec.FirstBytePos, uint64 Int64.MaxValue) |> int64

                            let lastBytePos = 
                                match byteRangeSpec.LastBytePos with
                                | Some length -> Math.Min(length, uint64 Int64.MaxValue) |> int64
                                | None -> resp.Entity.Length - 1L

                            let length = lastBytePos - firstBytePos

                            if length >= resp.Entity.Length then resp
                            else
                                let contentInfo =
                                    let contentRange = ByteContentRange.byteRangeResp (uint64 firstBytePos) (uint64 lastBytePos) resp.ContentInfo.Length
                                    resp.ContentInfo.With(range = Choice1Of2 contentRange, length = (uint64 length))
                                let entity = 
                                    match byteRangeSpec.LastBytePos with
                                    | Some _ -> resp.Entity |> Stream.subStream firstBytePos (int64 length)
                                    | None ->
                                        resp.Entity.Position <- firstBytePos
                                        resp.Entity
                                resp.With(entity, status = HttpStatus.successPartialContent, contentInfo = contentInfo)
                        | Choice2Of2 suffixByteRangeSpec::[] ->
                            let length =  Math.Min(suffixByteRangeSpec.ToUInt64(), uint64 Int64.MaxValue) |> int64
                            if length >= resp.Entity.Length then resp
                            else
                                let lastBytePos = resp.Entity.Length - 1L
                                let firstBytePos = lastBytePos - length

                                let contentInfo =
                                    let contentRange = ByteContentRange.byteRangeResp (uint64 firstBytePos) (uint64 lastBytePos) resp.ContentInfo.Length
                                    resp.ContentInfo.With(range = Choice1Of2 contentRange, length = (uint64 length))

                                resp.Entity.Position <- firstBytePos
                                resp.With(status = HttpStatus.successPartialContent, contentInfo = contentInfo)
                        | _::_ -> 
                            resp
                        | _ -> failwith "Invalid BytesRangeSpecifier: ByteRangeSet is empty"                
                    | _ -> resp.With(acceptedRanges = acceptedRanges)
            }
        }
(*
    [<CompiledName("ContentTypeNegotiating")>]
    let contentTypeNegotiating (parsers: seq<MediaType*Converter<Stream,'TReq>>, serializers: seq<MediaType*ResponseSerializer<'TResp>> ) (resource:IResource<'TReq,'TResp>) =
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
*)
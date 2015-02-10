namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open System
open System.IO
open FunctionalHttp.Core

type RequestConverterProvider<'TReq> = ContentInfo -> Option<Converter<Stream,'TReq>>
type ResponseConverterProvider<'TResp> = RequestPreferences -> Option<Converter<'TResp,Stream>>

type IStreamResource =
    abstract member Route:Route with get

    abstract member Process: HttpRequest<Stream> -> Async<HttpResponse<Stream>>

module StreamResource =
    [<CompiledName("Create")>]
    let create (getRequestConverter:RequestConverterProvider<'TReq>) (getResponseConverter:ResponseConverterProvider<'TResp>) (vary:Option<Choice<Set<Header>, Any>>) (resource:IResource<'TReq,'TResp>) = 
        let badRequestResponse = HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorBadRequest, None) |> async.Return
        let unsupportedMediaTypeResponse = HttpResponse<Option<'TResp>>.Create(HttpStatus.clientErrorUnsupportedMediaType, None) |> async.Return
        let notAcceptableResponse = HttpResponse<Stream>.Create(HttpStatus.clientErrorNotAcceptable, Stream.Null) |> async.Return

        { new IStreamResource with
            member this.Route = resource.Route

            member this.Process req = async {
                let req = resource.FilterRequest req
  
                let! resp = 
                    let reqWithoutEntity = req.With(())
                    resource.Handle reqWithoutEntity

                let! resp = 
                    if resp.Status <> HttpStatus.informationalContinue 
                    then resp |> async.Return
                    else 
                        match getRequestConverter req.ContentInfo with
                        | None -> unsupportedMediaTypeResponse
                        | Some converter -> async {
                            let! req = req |> HttpRequest.convert converter
                            return! 
                                match req.Entity with
                                | Choice1Of2 entity -> req.With(entity) |> resource.Accept 
                                | Choice2Of2 ex -> badRequestResponse
                        }
                let resp =
                    match vary with
                    | None -> resp
                    | Some vary -> resp.With(vary = vary)

                return! 
                    match (resp.Entity, getResponseConverter req.Preferences) with
                    | (Some entity, Some responseConverter)-> 
                        resp.With(entity) |> HttpResponse.convertOrThrow responseConverter
                    | (Some entity, None) ->
                        notAcceptableResponse
                    | (None, _) -> resp.With(Stream.Null) |> async.Return
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

    [<CompiledName("ContentTypeNegotiating")>]
    let contentTypeNegotiating (requestConverters:seq<MediaType*Converter<Stream,'TReq>>, responseConverters:seq<MediaType*Converter<'TResp,Stream>>) (resource:IResource<'TReq,'TResp>) =
        let getRequestConverter : RequestConverterProvider<'TReq> =
            let converters = Map.ofSeq requestConverters

            let getConverter (contentInfo:ContentInfo) =
                match contentInfo.MediaType with
                | None -> None
                | Some mediaType -> converters |> Map.tryFind mediaType

            getConverter       

        let getResponseConverter : ResponseConverterProvider<'TResp> =
            let converters = Map.ofSeq responseConverters
            let mediaTypes = converters |> Seq.map (fun kvp -> kvp.Key) |> List.ofSeq

            let getConverter (preferences:RequestPreferences) =
                match AcceptPreference.bestMatch mediaTypes preferences.AcceptedMediaRanges with
                | None -> None
                | Some mediaType -> converters |> Map.tryFind mediaType
                    
            getConverter
         
        let vary = Some (Choice1Of2 ([HttpHeaders.accept] |> Set.ofSeq))
        create getRequestConverter getResponseConverter vary resource     
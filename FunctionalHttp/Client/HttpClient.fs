namespace FunctionalHttp

open System
open System.IO
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks

type HttpClient<'TReq, 'TResp> = HttpRequest<'TReq> -> Async<HttpResponse<'TResp>>

type HttpClientConverter<'TReq,'TResp> = {
        // FIXME should serialization be async?
        SerializeRequest:HttpRequest<'TReq> -> HttpRequest<Stream>
        DeserializeResponse:HttpResponse<Stream> -> Async<HttpResponse<'TResp>>
    }

type RetryResult =
    | RetryAfter of timespan:TimeSpan
    | Continue

type RetryPolicy<'TResp> = (HttpResponse<'TResp>*int) -> RetryResult

module HttpClient =
    let usingContext (context:SynchronizationContext) (client:HttpClient<'TReq, 'TResp>) (request:HttpRequest<'TReq>) =
        async {
            do! Async.SwitchToContext context
            return! client request
        }

    let usingRetryPolicy (policy:RetryPolicy<'TResp>) (client:HttpClient<'TReq, 'TResp>) (request:HttpRequest<'TReq>) =
        let rec tryRequest (attempt:int) =
            async {
                let! response = client request
                match policy (response, attempt) with
                | Continue -> 
                    return response
                | RetryAfter ts -> 
                    do! Async.Sleep (int ts.TotalMilliseconds)
                    return! tryRequest (attempt+1)
            } 

        async {
            return! tryRequest 0
        }

    let usingConverter (converter:HttpClientConverter<'TReq,'TResp>) (client:HttpClient<Stream,Stream>) (request:HttpRequest<'TReq>) = 
        async {
            let streamRequest = converter.SerializeRequest request
            let! streamResponse = client streamRequest
            // FIXME: Maybe this should be configurable via a provided function
            // Only deserializer the response when the status class is Success and contains contentInfo
            return! match (streamResponse.Entity, 
                           streamResponse.Status.Class, 
                           streamResponse.ContentInfo, 
                           streamResponse.ContentInfo.Length) with
                    | (Some entity, statusClass, contentInfo, Some length) when 
                            statusClass = StatusClass.Success && 
                            contentInfo <> ContentInfo.None -> 
                        converter.DeserializeResponse <| streamResponse.With(entity)
                        //deserialize (streamResponse.With(entity = LimitReadStream.Create(entity, length)))
                    | (Some entity, statusClass, contentInfo, _) when 
                            statusClass = StatusClass.Success && 
                            contentInfo <> ContentInfo.None -> 
                        converter.DeserializeResponse streamResponse
                    | _ -> streamResponse.WithoutEntityAsync<'TResp>()
        } 

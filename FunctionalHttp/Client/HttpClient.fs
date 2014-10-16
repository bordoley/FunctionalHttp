namespace FunctionalHttp

open System
open System.Threading

type HttpClient<'TReq, 'TResp> = HttpRequest<'TReq> -> Async<HttpResponse<'TResp>>

type RetryResult =
    | RetryAfter of timespan:TimeSpan
    | Continue

type RetryPolicy<'TResp> = HttpResponse<'TResp> -> int -> RetryResult

module HttpClient =
    let UsingContext (context:SynchronizationContext) (client:HttpClient<'TReq, 'TResp>) (request:HttpRequest<'TReq>) =
        async {
            do! Async.SwitchToContext context
            return! client request
        }

    let RetryUsingPolicy (policy:RetryPolicy<'TResp>) (client:HttpClient<'TReq, 'TResp>) (request:HttpRequest<'TReq>) =
        let rec tryRequest (attempt:int) =
            async {
                let! response = client request
                match (policy response attempt) with
                | Continue -> 
                    return response
                | RetryAfter ts -> 
                    do! Async.Sleep (int ts.TotalMilliseconds)
                    return! tryRequest (attempt+1)
            } 

        async {
            return! tryRequest 0
        }

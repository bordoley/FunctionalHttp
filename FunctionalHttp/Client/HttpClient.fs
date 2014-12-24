namespace FunctionalHttp.Client

open FunctionalHttp.Core
open System.Threading

type HttpClient<'TReq, 'TResp> = HttpRequest<'TReq> -> Async<HttpResponse<'TResp>>

module HttpClient =
    let usingContext (context:SynchronizationContext) (client:HttpClient<'TReq, 'TResp>) (request:HttpRequest<'TReq>) =
        async {
            do! Async.SwitchToContext context
            return! client request
        }

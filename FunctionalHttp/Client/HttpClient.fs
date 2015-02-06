namespace FunctionalHttp.Client

open FunctionalHttp.Core
open System
open System.IO
open System.Threading

type internal HttpClient<'TReq, 'TResp> = HttpRequest<'TReq> -> Async<HttpResponse<'TResp>>

module HttpClient =
    [<CompiledName("UsingContext")>]
    let usingContext (context:SynchronizationContext) (client:HttpClient<'TReq, 'TResp>) =
        let handleRequest (request:HttpRequest<'TReq>) =
            async {
                do! Async.SwitchToContext context
                return! client request
            }
        handleRequest  

#if PCL
#else
    open System.Net
    open System.Net.Http

    [<CompiledName("FromNetHttpClient")>]
    let fromNetHttpClient (httpClient:System.Net.Http.HttpClient) =
        let handle (request: HttpRequest<Stream>) =
            async {
                let httpRequestMsg = request |> HttpRequest.toHttpRequestMessage

                // Explicitly don't let the client cache the response body. This should be left to
                // wrappers up the chain that parse the content to decided
                let! result = httpClient.SendAsync(httpRequestMsg, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask |> Async.Catch
                return! 
                    match result with 
                    | Choice1Of2 responseMessage -> responseMessage |> HttpResponse.fromHttpResponseMessageAsync
                    | Choice2Of2 exn -> 
                        let exn = 
                            match exn with
                            | :? AggregateException as exn -> (exn.Flatten()).InnerException
                            | _ -> exn

                        match exn with
                        | :? WebException as exn -> exn |> HttpResponse.fromWebException |> async.Return 
                        | _ -> Exception("Unexpected exception", exn) |> raise  
            }
        handle
#endif

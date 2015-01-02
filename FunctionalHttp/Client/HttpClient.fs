namespace FunctionalHttp.Client

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System
open System.IO
open System.Text
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
    open System.Runtime.CompilerServices

    [<CompiledName("AsFunctionalHttpClient"); Extension>]
    let asFunctionalHttpClient (httpClient:System.Net.Http.HttpClient) =
        let rec handleException (exn:exn) = 
            // FIXME: Add some logging

            match exn with
            | :? WebException as exn -> exn.ToResponse() |> Async.result          
            | :? AggregateException as exn -> 
                match exn.InnerException with 
                | null -> raise (Exception("Unexpected exception", exn))
                | exn -> handleException exn
            | _ -> raise (new System.Exception("Unexpected exception", exn))  

        fun (request: HttpRequest<Stream>) -> async {
            let httpClientRequest = request.ToHttpRequestMessage()

            // Explicitly don't let the client cache the response body. This should be left to
            // wrappers up the chain that parse the content to decided
            let! result = httpClient.SendAsync(httpClientRequest, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask |> Async.Catch
            return! match result with 
                    | Choice1Of2 responseMessage -> responseMessage.ToAsyncResponse()
                    | Choice2Of2 exn -> handleException exn
        }
#endif

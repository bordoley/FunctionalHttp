namespace FunctionalHttp

open FunctionalHttp

open System
open System.Net
open System.Net.Http
open System.IO
open System.Threading.Tasks

[<AutoOpen>]
module SystemNetHttpClientMixins = 
    type System.Net.Http.HttpClient with 
        // The only method we use is SendAsync which is threadsafe so let the caller provide a single
        // instance of HttpClient: http://msdn.microsoft.com/en-us/library/system.net.http.httpclient%28v=vs.110%29.aspx
        member this.AsFunctionalHttpClient () =
             fun (request: HttpRequest<Stream>) ->
                let rec handleException (exn:Exception) = 
                    // FIXME: Add some logging

                    match exn with
                    | :? WebException as exn -> exn.ToAsyncResponse()            
                    | :? AggregateException as exn ->
                        match exn.InnerException with 
                        | null -> raise (new System.Exception("Unexpected exception", exn))
                        | exn -> handleException exn
                    | _ -> raise (new System.Exception("Unexpected exception", exn))  
        
                async {
                    let httpClientRequest = request.ToHttpRequestMessage()
   
                    // Explicitly don't let the client cache the response body. This should be left to
                    // wrappers up the chain that parse the content to decided
                    let! result = this.SendAsync(httpClientRequest, HttpCompletionOption.ResponseHeadersRead) |> Async.AwaitTask |> Async.Catch
                    return! match result with 
                            | Choice1Of2 responseMessage -> responseMessage.ToAsyncResponse()
                            | Choice2Of2 exn -> handleException exn
                }

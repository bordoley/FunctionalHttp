namespace FunctionalHttp.Interop

open FunctionalHttp
open System
open System.IO
open System.Runtime.CompilerServices
open System.Threading
open System.Threading.Tasks

type HttpClient<'TReq, 'TResp> private (httpClient: FunctionalHttp.HttpClient<'TReq, 'TResp>) = 
    static member FromFSharpHttpClient(httpClient: FunctionalHttp.HttpClient<'TReq, 'TResp>) = 
        new HttpClient<'TReq, 'TResp>(httpClient); 

    member this.FSharpHttpClient with get() = httpClient

    member this.SendRequest(request:HttpRequest<'TReq>)  =
        let completer = TaskCompletionSource()

        let handleRequest = async {            
            let! response = httpClient request
            completer.SetResult(response)
        }

        Async.StartImmediate handleRequest
        completer.Task

[<AbstractClass; Sealed; Extension>]
type HttpClientInteropExtensions private () =
    [<Extension>]
    static member UsingContext(this:HttpClient<'TReq, 'TResp>, context:SynchronizationContext) =
        let client = HttpClient.usingContext context this.FSharpHttpClient
        HttpClient.FromFSharpHttpClient(client)

(*
    [<Extension>]
    static member UsingRetryPolicy(this:HttpClient<'TReq, 'TResp>,
                                    policy:Func<HttpResponse<'TResp>, int, RetryResult>) =
        let policyFunc(response, cnt) = policy.Invoke(response, cnt)

        let client = HttpClient.usingRetryPolicy policyFunc this.FSharpHttpClient
        HttpClient.FromFSharpHttpClient(client)

    [<Extension>]
    static member UsingConverter(this:HttpClient<Stream,Stream>,
                                 converter:HttpClientConverter<'TReq,'TResp>) =
        let client = HttpClient.usingConverter converter this.FSharpHttpClient
        HttpClient.FromFSharpHttpClient(client)
        *)
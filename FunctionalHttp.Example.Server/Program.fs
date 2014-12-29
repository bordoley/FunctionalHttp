namespace FunctionalHttp.Example.Server

open FunctionalHttp.Core
open FunctionalHttp.Server
open System
open System.IO
open System.Net

type EchoResource () =
    let route = Route.Create ["example"]

    interface IStreamResource with
        member this.Route = route
        member this.Filter (req: HttpRequest<obj>) = req
        member this.Filter (resp: HttpResponse<obj>) = resp
        member this.Handle (req:HttpRequest<obj>) = HttpStatus.successOk.ToAsyncResponse<obj>()
        member this.Accept (req: HttpRequest<obj>) = HttpStatus.successOk.ToAsyncResponse<obj>()
        member this.Parse (req: HttpRequest<Stream>) = req.WithoutEntityAsync<obj>()
        member this.Serialize (req:HttpRequest<_>, resp:HttpResponse<obj>) = resp.WithoutEntityAsync<Stream>()

module main =
    [<EntryPoint>]
    let main argv =
        printfn "%A" argv

        let listener = new HttpListener();
        listener.Prefixes.Add "http://*:8080/"

        let application = HttpApplication.singleResource (EchoResource() :> IStreamResource)

        async {

            let! server = HttpListenerServer.createAsync listener (fun _ -> application) |> Async.AwaitTask
            Console.ReadLine () |> ignore
            server.Dispose()
        } |> Async.RunSynchronously


        0 // return an integer exit code


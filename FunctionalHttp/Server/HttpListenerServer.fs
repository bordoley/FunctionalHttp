namespace FunctionalHttp.Server

open FunctionalHttp.Core
open FunctionalHttp.Parsing
open System
open System.IO
open System.Linq
open System.Net
open System.Reactive.Disposables
open System.Reactive.Linq
open System.Reactive.Threading.Tasks
open System.Threading
open System.Threading.Tasks

module HttpListenerServer =
    let private parseRequest (req:HttpListenerRequest) =
        let meth = Parser.parse Method.Parser req.HttpMethod |> Option.get
        let version = HttpVersion.Create(req.ProtocolVersion.Major, req.ProtocolVersion.Minor)
        let headers = (req.Headers.AllKeys :> seq<string>) |> Seq.map(fun key -> (key, req.Headers.GetValues(key) :> string seq))

        HttpRequest<Stream>.Create(meth, req.Url, version, Some req.InputStream, headers)

    let private sendResponse (listenerResponse:HttpListenerResponse) (resp:HttpResponse<Stream>) =
        listenerResponse.StatusCode <- resp.Status.Code
        async {
            do!
                match resp.Entity with
                | Some stream -> stream.CopyToAsync(listenerResponse.OutputStream) |> Async.AwaitIAsyncResult |> Async.Ignore
                | _ -> async { return () }
            listenerResponse.Close()
        }

    let create (listener:HttpListener) (applicationProvider:HttpRequest<_> -> IHttpApplication)  : IDisposable =
        let listenAndProcessRequest (ctx:HttpListenerContext) =
            async {
                try

                    do! Async.SwitchToThreadPool()
                    let req = parseRequest ctx.Request 
                    let! resp = HttpServer.processRequest applicationProvider req
                    do! sendResponse ctx.Response resp
                with
                    | ex -> ()
                ()
            }
        
        listener.Start ()

        let retval = new CompositeDisposable()
        retval.Add(
            Observable.Defer(fun () -> 
                    listener.GetContextAsync().ToObservable())
                .Repeat()
                .Do(fun ctx -> 
                    listenAndProcessRequest ctx |> Async.StartImmediate)
                .Subscribe())
        retval.Add (Disposable.Create (fun () -> listener.Stop()))
        retval :> IDisposable

    let createAsync (listener:HttpListener) (applicationProvider:HttpRequest<_> -> IHttpApplication) : Task<IDisposable> =
        let tcs = TaskCompletionSource<IDisposable>()

        let threadStart () =
            let eventLoop = EventLoop.current ()

            let disposable = new CompositeDisposable()
            disposable.Add (create listener applicationProvider)
            disposable.Add (eventLoop :> IDisposable)
            tcs.SetResult (disposable :> IDisposable)

            eventLoop.Run ()


        let thread = Thread(fun () -> threadStart())
        thread.Start ()

        tcs.Task
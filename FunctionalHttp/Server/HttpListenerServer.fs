namespace FunctionalHttp

open FunctionalHttp
open System
open System.Net
open System.Reactive.Disposables
open System.Reactive.Linq
open System.Reactive.Threading.Tasks

module HttpServer =
    let create (applicationProvider:HttpRequest<_> -> IHttpApplication) (listener:HttpListener) : IDisposable =
        let listenAndProcessRequest (ctx:HttpListenerContext) =
            async {
               
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

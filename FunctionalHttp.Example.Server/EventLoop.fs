namespace FunctionalHttp.Example.Server
open System
open System.Collections.Concurrent
open System.Threading
open System.Threading.Tasks

type internal IEventLoop =
    inherit IDisposable

    abstract member PostAsync: (unit -> 'T)*CancellationToken -> Task<'T>
    abstract member Run : unit -> unit

[<Sealed>]
type internal EventLoopSynchronizationContext(eventLoop:IEventLoop) =
    inherit SynchronizationContext()

    override this.Post(d:SendOrPostCallback, state:obj) =
        eventLoop.PostAsync ((fun () -> d.Invoke(state)), CancellationToken.None) |> ignore

[<Sealed>]
type internal EventLoopImpl () = 
    let queue = new BlockingCollection<(unit -> unit)*CancellationToken>()
    let disposed = new CancellationTokenSource()
    let thread = Thread.CurrentThread
    let running = ref false

    interface IDisposable with
        member this.Dispose () = disposed.Cancel ()

    interface IEventLoop with
        member this.PostAsync (f:unit -> 'T, cts:CancellationToken) =
            if disposed.Token.IsCancellationRequested then raise (ObjectDisposedException("object disposed"))

            let tcs = TaskCompletionSource()

            let action () = 
                try f () |> tcs.SetResult 
                with | ex -> tcs.SetException ex

            queue.Add((action, cts))
            tcs.Task
       
        member this.Run () =
            if disposed.Token.IsCancellationRequested then raise (ObjectDisposedException("object disposed"))
            if Thread.CurrentThread <> thread then raise (InvalidOperationException("Attempting to run the event loop on a different thread than it was created on"))
            if !running then raise (InvalidOperationException("event loop is already running"))

            running := true

            SynchronizationContext.SetSynchronizationContext(EventLoopSynchronizationContext(this :> IEventLoop))
            let rec loop () =
                if disposed.Token.IsCancellationRequested
                then ()
                else
                    try
                        let (action,cts) = queue.Take(disposed.Token)
                        if not cts.IsCancellationRequested then action()
                        loop()
                    with
                    | :? OperationCanceledException -> ()

            loop ()             

module internal EventLoop =
    let private _current = new ThreadLocal<IEventLoop>(fun () -> new EventLoopImpl() :> IEventLoop)
    let current () = _current.Value
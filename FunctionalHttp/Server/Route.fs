namespace FunctionalHttp.Server
open System

type Route = 
    private {
        route:string list
    }

    member this.ToList() = this.route

    // FIXME: Quick hack
    static member Create (route:string list) = { route = route }
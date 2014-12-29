namespace FunctionalHttp.Server
open System

type Route = 
    private {
        route:string list
    }

    // FIXME: Quick hack
    static member Create (route:string list) = { route = route }


    member this.ToList() = this.route

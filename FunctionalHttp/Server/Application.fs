namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System.IO

type IHttpApplication =
    abstract Filter:  HttpRequest<Stream> -> HttpRequest<Stream>
    abstract Filter: HttpResponse<Stream> -> HttpResponse<Stream>
    abstract Route: HttpRequest<Stream> -> IStreamResource

type internal RoutingApplication (router:Router, defaultResource:IStreamResource) =
    interface IHttpApplication with
        member this.Filter (req:HttpRequest<Stream>)= req
        member this.Filter (resp:HttpResponse<Stream>) = resp
        member this.Route (req:HttpRequest<Stream>) = 
            let path = List.ofArray req.Uri.Segments
            (router.Item path) |> Option.getOrElse defaultResource
     
module HttpApplication =
    [<CompiledName("SingleResource")>]
    let singleResource resource = 
        { new IHttpApplication with
            member this.Filter (req:HttpRequest<Stream>)= req
            member this.Filter (resp:HttpResponse<Stream>) = resp
            member this.Route (req:HttpRequest<Stream>) = resource
        }

    [<CompiledName("Routing")>]
    let routing (resources, defaultResource)  =
        RoutingApplication ((Router.Empty.AddAll resources), defaultResource) :> IHttpApplication

    [<CompiledName("WithFilters")>]
    let withFilters (requestFilter:RequestFilter<Stream>, responseFilter:ResponseFilter<Stream>)  (application:IHttpApplication) =
        { new IHttpApplication with
            member this.Filter (req:HttpRequest<Stream>)= requestFilter req
            member this.Filter (resp:HttpResponse<Stream>) = application.Filter resp
            member this.Route (req:HttpRequest<Stream>) = application.Route req
        }
namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System.IO

type IHttpApplication =
    abstract Filter:  HttpRequest<Stream> -> HttpRequest<Stream>
    abstract Filter: HttpResponse<Stream> -> HttpResponse<Stream>
    abstract Route: HttpRequest<Stream> -> IStreamResource

type internal SingleResourceApplication (resource:IStreamResource) =
    interface IHttpApplication with
        member this.Filter (req:HttpRequest<Stream>)= req
        member this.Filter (resp:HttpResponse<Stream>) = resp
        member this.Route (req:HttpRequest<Stream>) = resource

type internal RoutingApplication (router:Router, defaultResource:IStreamResource) =
    interface IHttpApplication with
        member this.Filter (req:HttpRequest<Stream>)= req
        member this.Filter (resp:HttpResponse<Stream>) = resp
        member this.Route (req:HttpRequest<Stream>) = 
            let path = List.ofArray req.Uri.Segments
            (router.Item path) |> Option.getOrElse defaultResource

type internal RequestFilterApplication (application:IHttpApplication, requestFilter:RequestFilter<Stream>) =
    interface IHttpApplication with
        member this.Filter (req:HttpRequest<Stream>)= requestFilter req
        member this.Filter (resp:HttpResponse<Stream>) = application.Filter resp
        member this.Route (req:HttpRequest<Stream>) = application.Route req

type internal ResponseFilterApplication (application:IHttpApplication, responseFilter:ResponseFilter<Stream>) =
    interface IHttpApplication with
        member this.Filter (req:HttpRequest<Stream>)= application.Filter req
        member this.Filter (resp:HttpResponse<Stream>) = responseFilter resp
        member this.Route (req:HttpRequest<Stream>) = application.Route req
     
module HttpApplication =
    [<CompiledName("SingleResource")>]
    let singleResource resource = SingleResourceApplication(resource) :> IHttpApplication

    [<CompiledName("Routing")>]
    let routing (resources, defaultResource)  =
        RoutingApplication ((Router.Empty.AddAll resources), defaultResource) :> IHttpApplication

    [<CompiledName("WithRequestFilter")>]
    let withRequestFilter (filter:RequestFilter<Stream>) (application:IHttpApplication) =
        RequestFilterApplication(application, filter) :> IHttpApplication

    [<CompiledName("WithResponseFilter")>]
    let withResponseFilter (filter:ResponseFilter<Stream>) (application:IHttpApplication) =
        ResponseFilterApplication(application, filter) :> IHttpApplication
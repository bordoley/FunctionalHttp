namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core

open System
open System.IO

type IHttpApplication =
    abstract FilterRequest: HttpRequest<Stream> -> HttpRequest<Stream>
    abstract FilterResponse: HttpResponse<Stream> -> HttpResponse<Stream>
    abstract Route: HttpRequest<Stream> -> IStreamResource
     
module HttpApplication =
    [<CompiledName("SingleResource")>]
    let singleResource resource = 
        { new IHttpApplication with
            member this.FilterRequest (req:HttpRequest<Stream>)= req
            member this.FilterResponse (resp:HttpResponse<Stream>) = resp
            member this.Route (req:HttpRequest<Stream>) = resource
        }

    [<CompiledName("Routing")>]
    let routing (resources, defaultResource)  =
        let router = Router.empty.AddAll resources

        { new IHttpApplication with
            member this.FilterRequest (req:HttpRequest<Stream>)= req
            member this.FilterResponse (resp:HttpResponse<Stream>) = resp
            member this.Route (req:HttpRequest<Stream>) = 
                // FIXME: Add uri extensions Uri.ToPath()
                let path = req.Uri.AbsolutePath.Split ([|'/'|], StringSplitOptions.None) |> Seq.toList
                (router.Item path) |> Option.getOrElse defaultResource
        }

    [<CompiledName("WithFilters")>]
    let withFilters (requestFilter:RequestFilter<Stream>, responseFilter:ResponseFilter<Stream>)  (application:IHttpApplication) =
        { new IHttpApplication with
            member this.FilterRequest (req:HttpRequest<Stream>)= requestFilter req
            member this.FilterResponse (resp:HttpResponse<Stream>) = responseFilter resp
            member this.Route (req:HttpRequest<Stream>) = application.Route req
        }
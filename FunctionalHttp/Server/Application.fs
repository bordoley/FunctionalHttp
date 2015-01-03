namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core

open System.IO

type IHttpApplication =
    abstract FilterRequest: HttpRequest<Stream> -> HttpRequest<Stream>
    abstract FilterResponse: HttpResponse<Stream> -> HttpResponse<Stream>
    abstract Route: HttpRequest<Stream> -> IServerResource
     
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
        let router = Router.Empty.AddAll resources

        { new IHttpApplication with
            member this.FilterRequest (req:HttpRequest<Stream>)= req
            member this.FilterResponse (resp:HttpResponse<Stream>) = resp
            member this.Route (req:HttpRequest<Stream>) = 
                let path = List.ofArray req.Uri.Segments
                (router.Item path) |> Option.getOrElse defaultResource
        }

    [<CompiledName("WithFilters")>]
    let withFilters (requestFilter:RequestFilter<Stream>, responseFilter:ResponseFilter<Stream>)  (application:IHttpApplication) =
        { new IHttpApplication with
            member this.FilterRequest (req:HttpRequest<Stream>)= requestFilter req
            member this.FilterResponse (resp:HttpResponse<Stream>) = responseFilter resp
            member this.Route (req:HttpRequest<Stream>) = application.Route req
        }
namespace FunctionalHttp.Server

open FunctionalHttp.Core
open System
open System.IO

type IResource<'TReq> = 
    abstract member Route:Route with get

    abstract FilterRequest: HttpRequest<_> -> HttpRequest<_>
    abstract FilterResponse: HttpResponse<_> -> HttpResponse<_>

    abstract Handle: HttpRequest<_> -> Async<HttpResponse<_>>
    abstract Accept: HttpRequest<'TReq> -> Async<HttpResponse<_>>

 type IStreamResource<'TReq> =
     inherit IResource<'TReq>

     abstract Parse: HttpRequest<Stream> -> Async<HttpRequest<'TReq>>
     abstract Serialize: HttpRequest<_>*HttpResponse<_> -> Stream -> Async

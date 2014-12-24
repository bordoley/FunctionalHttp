namespace FunctionalHttp.Server

open FunctionalHttp.Core
open System.IO

type IHttpApplication =
    abstract FilterRequest: HttpRequest<_> -> HttpRequest<_>
    abstract FilterResponse: HttpResponse<_> -> HttpResponse<_>
    abstract Route: HttpRequest<_> -> IStreamResource<_>
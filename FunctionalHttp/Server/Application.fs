namespace FunctionalHttp

open FunctionalHttp
open System.IO

type IHttpApplication =
    abstract FilterRequest: HttpRequest<_> -> HttpRequest<_>
    abstract FilterResponse: HttpResponse<_> -> HttpResponse<_>
    abstract Route: HttpRequest<_> -> IStreamResource<_>
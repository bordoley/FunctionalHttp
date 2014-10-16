namespace FunctionalHttp

open System.IO

type RequestSerializer<'TReq> = HttpRequest<'TReq> -> HttpRequest<Stream>

module RequestSerializer =
    let None (request:HttpRequest<_>) = request.WithoutEntity<Stream>()

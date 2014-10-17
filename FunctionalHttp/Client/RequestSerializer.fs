namespace FunctionalHttp

open System.IO

type RequestSerializer<'TReq> = HttpRequest<'TReq> -> HttpRequest<Stream>

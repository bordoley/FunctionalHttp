namespace FunctionalHttp.Server

open FunctionalHttp.Core
open System.IO

type IServerResource =
    inherit IResource

    abstract member Parse: HttpRequest<Stream> -> Async<HttpRequest<Choice<obj,exn>>>
    abstract member Serialize: HttpRequest<_>*HttpResponse<obj> -> Async<HttpResponse<Stream>>
 
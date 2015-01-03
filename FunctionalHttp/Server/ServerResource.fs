namespace FunctionalHttp.Server

open FunctionalHttp.Core
open System.IO

type IServerResource =
    inherit IResource

    abstract member Parse: HttpRequest<Stream> -> Async<HttpRequest<Choice<obj,exn>>>
    abstract member Serialize: HttpRequest<unit>*HttpResponse<obj> -> Async<HttpResponse<Stream>>

module ServerResource =
    [<CompiledName("Create")>]
    let create (parse, serialize) (resource:IResource) =
        {new IServerResource with
            member this.Route = resource.Route

            member this.Filter (req: HttpRequest<unit>) = resource.Filter req
    
            member this.Filter (resp: HttpResponse<obj>) = resource.Filter resp

            member this.Handle req = resource.Handle req

            member this.Accept req = resource.Accept req

            member this.Parse req = parse req

            member this.Serialize (req, resp) = serialize (req, resp)
        }
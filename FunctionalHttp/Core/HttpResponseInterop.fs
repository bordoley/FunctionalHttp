namespace FunctionalHttp.Interop

open FunctionalHttp
open FunctionalHttp.HttpStreamResponseDeserializers
open System.IO
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type HttpResponseExtensions private () =
    [<Extension>]
    static member ToResponse(this:Status) = this.ToResponse()

    [<Extension>]
    static member ToAsyncResponse(this:Status) = this.ToAsyncResponse()

    [<Extension>]
    static member ToAsyncResponse(this:HttpResponse<'TResp>) = this.ToAsyncResponse()

    [<Extension>]
    static member WithoutEntityAsyncResponse<'TResp> (this:HttpResponse<_>) = this.WithoutEntityAsync<'TResp>()

    [<Extension>]
    static member ToAsyncMemoryStreamResponse(this:HttpResponse<Stream>) = ToAsyncMemoryStreamResponse this

    [<Extension>]
    static member ToAsyncByteArrayResponse(this:HttpResponse<Stream>) = ToAsyncByteArrayResponse this

    [<Extension>]
    static member ToAsyncStringResponse (this:HttpResponse<Stream>) = ToAsyncStringResponse this

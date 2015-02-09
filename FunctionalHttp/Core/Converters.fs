namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open System
open System.IO
open System.Text

type Converter<'TIn, 'TOut> = ContentInfo*'TIn -> Async<ContentInfo*'TOut>

module Converters =
    [<CompiledName("Bind")>]
    let bind (b:Converter<'TIntermediate, 'TOut>) (a:Converter<'TIn, 'TIntermediate>) =
        let composed (contentInfo, input) = (contentInfo, input) |> a |> Async.bind b
        composed

    [<CompiledName("FromStreamToString")>]
    let fromStreamToString (contentInfo:ContentInfo, stream:Stream) =
        async {
            let encoding = 
                contentInfo.MediaType 
                 |> Option.bind (fun mr -> mr.Charset) 
                 |> Option.bind (fun charset -> charset.Encoding)
                 |> Option.getOrElse Encoding.UTF8 

            use sr = new StreamReader(stream, encoding)
            let! result = sr.ReadToEndAsync() |> Async.AwaitTask 
            return (contentInfo, result)
        }

    [<CompiledName("FromStreamToMemoryStream")>]
    let fromStreamToMemoryStream (contentInfo:ContentInfo, stream:Stream) =
        async {
            let memStream =
                match contentInfo.Length with
                | Some length -> 
                    let byteArray = Array.init<byte> (int length) (fun i -> 0uy)
                    new MemoryStream(byteArray)
                | _ -> new MemoryStream()

            let! copyResult = stream.CopyToAsync(memStream) |> Async.AwaitIAsyncResult
            return (contentInfo, memStream)
        }

    [<CompiledName("FromStreamToByteArray")>]
    let fromStreamToByteArray (contentInfo:ContentInfo, stream:Stream) =
        async {
            let! (contentInfo, stream) = fromStreamToMemoryStream(contentInfo, stream)
            return (contentInfo, (stream.ToArray()))
        }

    [<CompiledName("FromStringToStream")>]
    let fromStringToStream (contentInfo:ContentInfo, str:string) =
        async {
            let bytes = Encoding.UTF8.GetBytes(str.ToString().ToCharArray())

            // FIXME: ContentInfo is incomplete here
            let contentInfo = contentInfo.With(length = uint64 bytes.Length)
            let stream = new MemoryStream(bytes) :> Stream
            return (contentInfo, stream)
        }

    [<CompiledName("FromUnitToStream")>] 
    let fromUnitToStream (contentInfo:ContentInfo, unit:unit) =
        async {
            return (ContentInfo.None, Stream.Null)
        }

    [<CompiledName("FromAnyToString")>]
    let fromAnyToString (contentInfo:ContentInfo, obj:_) =
        (contentInfo, obj.ToString()) |> async.Return

    [<CompiledName("FromAnyToObject")>]
    let fromAnyToObject (contentInfo:ContentInfo, obj:_) =
        (contentInfo, obj :> obj) |> async.Return
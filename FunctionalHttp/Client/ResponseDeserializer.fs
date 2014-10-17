namespace FunctionalHttp

open System.IO
open System.Runtime.CompilerServices
open System.Text

type ResponseDeserializer<'TResp> = HttpResponse<Stream> -> Async<HttpResponse<'TResp>>

[<AutoOpen; Extension>]
module ResponseDeserializerExtensions =
    [<Extension>]
    let ToAsyncMemoryStreamResponse(this:HttpResponse<Stream>) =
        let stream = this.Entity.Value
        async{
            let memStream =
                match this.ContentInfo.Length with
                | Some length -> 
                    let byteArray = Array.init<byte> (int length) (fun i -> 0uy)
                    new MemoryStream(byteArray)
                | _ -> new MemoryStream()

            let! copyResult = stream.CopyToAsync(memStream) |> Async.AwaitIAsyncResult |> Async.Catch
            return match copyResult with
                    | Choice1Of2 unit -> this.With(entity = memStream)
                    | Choice2Of2 exn -> HttpResponseExtensions.ToResponse(ClientStatus.DeserializationFailed).With(id = this.Id)
        }

    [<Extension>]
    let ToAsyncByteArrayResponse(this:HttpResponse<Stream>) =
        async {
            let! memResponse = ToAsyncMemoryStreamResponse(this)
            return
                match memResponse.Entity with
                | None -> memResponse.WithoutEntity<byte[]>()
                | Some stream -> this.With(entity = stream.ToArray())    
        }

    [<Extension>]
    let ToAsyncStringResponse (this:HttpResponse<Stream>)  =
        let stream = this.Entity.Value
        async {
            let encoding = 
                match 
                    this.ContentInfo.MediaRange 
                    |> Option.bind (fun mr -> mr.Charset) 
                    |> Option.bind (fun charset -> charset.Encoding)  with
                | Some enc -> enc
                | _ -> Encoding.UTF8

            use sr = new StreamReader(stream, encoding)

            let! result = sr.ReadToEndAsync() |> Async.AwaitTask |> Async.Catch
            return 
                match result with
                | Choice2Of2 exn ->
                    ClientStatus.DeserializationFailed.ToResponse<string>().With(id = this.Id)
                | Choice1Of2 result ->
                    this.With<string>(result)
        }

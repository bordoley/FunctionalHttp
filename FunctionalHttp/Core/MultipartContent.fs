namespace FunctionalHttp.Core
open System
open System.Linq
open System.IO
open System.Text
open System.Threading.Tasks

module internal Stream =
    type private SubStream (stream:Stream, start:int64, length:int64) =
        inherit Stream()

        let disposed = ref false
        let started = ref false

        override this.Dispose(disposing) =
            match (!disposed, disposing) with
            | (true, _) -> ()
            | (false, true) ->
                stream.Dispose()
                disposed := true
                base.Dispose(disposing)
            | _ ->
                disposed := true
                base.Dispose(disposing)
            
        override this.CanRead = stream.CanRead

        override this.CanSeek = false

        override this.CanWrite = false

        override this.Length = length

        override this.Position
            with get() =
                 stream.Position - start
            and set(value) = 
                 NotSupportedException() |> raise 

        override this.Flush() = stream.Flush ()

        override this.FlushAsync ct = stream.FlushAsync ct

        override this.Seek(offset, origin) =
            NotSupportedException() |> raise 
             
        override this.SetLength(value) =
            NotSupportedException() |> raise 

        override this.Write(byte, offset, count) = 
            NotSupportedException() |> raise 

        override this.WriteAsync(buffer, offset, count, cancellationToken) = 
            NotSupportedException() |> raise 

        override this.Read(buffer, offset, count) =
            if !disposed then ObjectDisposedException(this.GetType().FullName) |> raise

            if not !started then stream.Position <- start

            if this.Position >= length then 0
            else
                let count = 
                    let numBytes = Math.Min(length - this.Position, (int64 count))
                    Math.Min(numBytes, (int64 Int32.MaxValue)) |> int32
                stream.Read(buffer, offset, count)

        override this.ReadAsync(buffer, offset, count, cancellationToken) =
            if !disposed then ObjectDisposedException(this.GetType().FullName) |> raise

            if not !started then stream.Position <- start

            if this.Position >= length then Task.FromResult 0
            else
                let count = 
                    let numBytes = Math.Min(length - this.Position, (int64 count))
                    Math.Min(numBytes, (int64 Int32.MaxValue)) |> int32
                stream.ReadAsync(buffer, offset, count)

    type private CombineStream (parts:Stream[]) =
        inherit Stream()

        let index = ref 0
        let disposed = ref false

        override this.Dispose(disposing) =
            match (!disposed, disposing) with
            | (true, _) -> ()
            | (false, true) ->
                for part in parts do part.Dispose()
                disposed := true
                base.Dispose(disposing)
            | _ ->
                disposed := true
                base.Dispose(disposing)
            
        override this.CanRead = true

        override this.CanSeek = false

        override this.CanWrite = false

        override this.Length =
            NotSupportedException() |> raise 

        override this.Position
            with get() =
                 NotSupportedException() |> raise 
            and set(value) = 
                 NotSupportedException() |> raise 

        override this.Flush() = ()

        override this.Seek(offset, origin) =
            NotSupportedException() |> raise 
             
        override this.SetLength(value) =
            NotSupportedException() |> raise 

        override this.Write(byte, offset, count) = 
            NotSupportedException() |> raise 

        override this.WriteAsync(buffer, offset, count, cancellationToken) = 
            NotSupportedException() |> raise 

        override this.Read(buffer, offset, count) =
            if !disposed then ObjectDisposedException(this.GetType().FullName) |> raise

            let rec doLoop (result:int)  (offset:int) (count:int)  =
                if count <= 0 || !index = parts.Length then result
                else
                    let bytesRead = parts.[!index].Read (buffer, offset, count)
                    let result = result + bytesRead
                    let offset = offset + bytesRead
                    let count = count - bytesRead

                    if count > 0 && !index < parts.Length then index := !index + 1

                    doLoop result offset count       

            doLoop 0 offset count

        override this.ReadAsync(buffer, offset, count, cancellationToken) =
            if !disposed then ObjectDisposedException(this.GetType().FullName) |> raise

            let tcs = TaskCompletionSource<int> ()

            let rec doLoop (result:int) (offset:int) (count:int)  =
                async {
                    if count <= 0 || !index = parts.Length then return result
                    else
                        let! bytesRead = parts.[!index].ReadAsync (buffer, offset, count, cancellationToken) |> Async.AwaitTask
                        let result = result + bytesRead
                        let offset = offset + bytesRead
                        let count = count - bytesRead

                        if count > 0 && !index < parts.Length then index := !index + 1

                        return! doLoop result offset count       
                }

            async {
                let! result = doLoop 0 offset count
                tcs.SetResult result
            } |> Async.StartImmediate
            
            tcs.Task

    let combined (streams:#seq<Stream>) =
        new CombineStream(Seq.toArray streams) :> Stream

    let fromString (encoding:Encoding) (str:string) =
        str |> encoding.GetBytes |> (fun bytes -> new MemoryStream (bytes)) :> Stream

    let subStream (start:int64) (length:int64) (stream:Stream) =
        if (not stream.CanSeek) && (not stream.CanRead) then ArgumentException() |> raise
        new SubStream(stream, start, length) :> Stream

    let asciiStreamFromString =
        Encoding.GetEncoding("ascii") |> fromString

    let memoryStream (bytes:byte[]) = new MemoryStream (bytes) :> Stream

type MultipartContent = 
    private {
        contentInfo:ContentInfo
        headers:Map<Header,obj>
        entity:Stream
    }

    member this.ContentInfo = this.contentInfo
    member this.Headers = this.headers
    member this.Entity = this.entity

    override this.ToString() =
        let builder = StringBuilder()
        let writeHeaderLine = Header.headerLineFunc builder
        this |> MultipartContent.WriteHeaders writeHeaderLine

        string builder

    static member internal WriteHeaders (f:Header*string -> unit) (content:MultipartContent) =
        content.ContentInfo |> ContentInfo.WriteHeaders f
        content.Headers |> Map.toSeq |> Header.writeAll f

type MultipartBoundary =
    private { bytes: byte[] }

    override this.ToString() = 
        Encoding.GetEncoding("ascii").GetString(this.bytes, 0, this.bytes.Length)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MultipartBoundary =
    // See: http://www.w3.org/Protocols/rfc1341/7_2_Multipart.html
    let private bcharsnospace = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789'()+_,-./:=?".ToCharArray()
    let private rng = new Random()

    let generate () =
        let bytes =
            Enumerable.Repeat(bcharsnospace, 70) 
            // Cheat: We know that all char values are less 8bit
            |> Seq.map (fun s -> (byte bcharsnospace.[rng.Next(s.Length)]))
            |> Seq.toArray
        { bytes = bytes }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal MultipartContent =
    let private dash_dash =
        let bytes = Array.create 2 0uy
        bytes.[0] <- (byte '-')
        bytes.[1] <- (byte '-')
        bytes

    let private crlf =
        let bytes = Array.create 2 0uy
        bytes.[0] <- (byte '\r')
        bytes.[1] <- (byte '\n')
        bytes

    let private crlf_crlf =
        let bytes = Array.create 4 0uy
        bytes.[0] <- (byte '\r')
        bytes.[1] <- (byte '\n')
        bytes.[3] <- (byte '\r')
        bytes.[4] <- (byte '\n')
        bytes

    let private toStream (boundary:MultipartBoundary) (multipartContent : MultipartContent) = 
        [ 
          Stream.memoryStream dash_dash;
          Stream.memoryStream boundary.bytes
          Stream.memoryStream crlf
          Stream.asciiStreamFromString (string multipartContent); 
          multipartContent.Entity; 
          Stream.memoryStream crlf_crlf
        ] |> Stream.combined

    let createStream (boundary:MultipartBoundary) (parts:MultipartContent list) =
        let finishBoundary =
            [
                Stream.memoryStream dash_dash;
                Stream.memoryStream boundary.bytes;
                Stream.memoryStream dash_dash;
                Stream.memoryStream crlf
            ] |> Stream.combined

        let parts = 
            let streams = parts |> Seq.map (toStream boundary)
            Seq.append streams [finishBoundary] 

        Stream.combined parts
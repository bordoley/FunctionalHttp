namespace FunctionalHttp.Core

open System
open System.Text

type RequestPreconditions =
    private {
        ifMatch: Choice<Set<EntityTag>, Any> option
        ifModifiedSince: DateTime option
        ifNoneMatch: Choice<Set<EntityTag>, Any> option
        ifUnmodifiedSince: DateTime option
        ifRange: Choice<EntityTag, DateTime> option
    }

    member this.IfMatch with get() = this.ifMatch
    member this.IfModifiedSince with get() = this.ifModifiedSince
    member this.IfNoneMatch with get() = this.ifNoneMatch
    member this.IfUnmodifiedSince with get() = this.ifUnmodifiedSince
    member this.IfRange with get() = this.ifRange

    override this.ToString() =
        let builder = StringBuilder()
        let writeHeaderLine = Header.headerLineFunc builder

        this |> RequestPreconditions.WriteHeaders writeHeaderLine

        string builder
    
    static member None = RequestPreconditionsHelper.None

    static member internal CreateInternal(ifMatch, ifModifiedSince, ifNoneMatch, ifUnmodifiedSince, ifRange) =
        match (ifMatch, ifModifiedSince, ifNoneMatch, ifUnmodifiedSince, ifRange)  with
        | (None, None, None, None, None) -> RequestPreconditions.None
        | (ifMatch, ifModifiedSincel, ifNoneMatch, ifUnmodifiedSince, ifRange) ->
            {   ifMatch = ifMatch; 
                ifModifiedSince = ifModifiedSince; 
                ifNoneMatch = ifNoneMatch;
                ifUnmodifiedSince = ifUnmodifiedSince;
                ifRange = ifRange }

    static member Create(?ifMatch, ?ifModifiedSince, ?ifNoneMatch, ?ifUnmodifiedSince, ?ifRange) =
        RequestPreconditions.CreateInternal(ifMatch, ifModifiedSince, ifNoneMatch, ifUnmodifiedSince, ifRange)

    static member Create(headers:Map<Header, obj>) = 
        let ifMatch = HeaderParsers.ifMatch headers
        let ifModifiedSince = HeaderParsers.ifModifiedSince headers
        let ifNoneMatch = HeaderParsers.ifNoneMatch headers
        let ifUnmodifiedSince = HeaderParsers.ifUnmodifiedSince headers
        let ifRange = HeaderParsers.ifRange headers

        RequestPreconditions.CreateInternal(ifMatch, ifModifiedSince, ifNoneMatch, ifUnmodifiedSince, ifRange)

    static member internal WriteHeaders (f:Header*string -> unit) (preconditions:RequestPreconditions) = 
        (HttpHeaders.ifMatch, preconditions.ifMatch) 
        |> function
            | (header, Some (Choice1Of2 etags)) -> (header, etags :> obj)
            | (header, Some (Choice2Of2 any)) -> (header, any :> obj)
            | (header, _) -> (header, "" :> obj)
        |> Header.writeObject f

        (HttpHeaders.ifModifiedSince, preconditions.ifModifiedSince) |> Header.writeDateTime f

        (HttpHeaders.ifMatch, preconditions.ifNoneMatch) 
        |> function
            | (header, Some (Choice1Of2 etags)) -> (header, etags :> obj)
            | (header, Some (Choice2Of2 any)) -> (header, any :> obj)
            | (header, _) -> (header, "" :> obj)
        |> Header.writeObject f

        (HttpHeaders.ifUnmodifiedSince, preconditions.ifUnmodifiedSince) |> Header.writeDateTime f

        (HttpHeaders.ifRange, preconditions.ifRange)
        |> function
            | (header, Some (Choice1Of2 etag)) -> (header, etag) |> Header.writeObject f
            | (header, Some (Choice2Of2 date)) -> (header, Some date) |> Header.writeDateTime f
            | _ -> ()

and [<AbstractClass; Sealed;>] internal RequestPreconditionsHelper () =
    static member val None : RequestPreconditions = { ifMatch = None; ifModifiedSince = None; ifNoneMatch = None; ifUnmodifiedSince = None; ifRange = None }

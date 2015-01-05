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

    override this.ToString() =
        let builder = StringBuilder()
        let writeHeaderLine = HeaderInternal.headerLineFunc builder

        this |> RequestPreconditions.WriteHeaders writeHeaderLine

        string builder
    
    static member None = { ifMatch = None; ifModifiedSince = None; ifNoneMatch = None; ifUnmodifiedSince = None; ifRange = None }

    static member Create(headers:Map<Header, obj>) = 
        let ifMatch = HeaderParsers.ifMatch headers

        // FIXME:
        let ifModifiedSince = None
        let ifNoneMatch = HeaderParsers.ifNoneMatch headers

        // FIXME:
        let ifUnmodifiedSince = None
        let ifRange = None


        {   ifMatch = ifMatch; 
            ifModifiedSince = ifModifiedSince; 
            ifNoneMatch = ifNoneMatch;
            ifUnmodifiedSince = ifUnmodifiedSince;
            ifRange = ifRange }

    static member internal WriteHeaders (f:string*string -> unit) (preconditions:RequestPreconditions) = 
        (HttpHeaders.ifMatch, preconditions.ifMatch) 
        |> function
            | (header, Some (Choice1Of2 etags)) -> (header, etags :> obj)
            | (header, Some (Choice2Of2 any)) -> (header, any :> obj)
            | (header, _) -> (header, "" :> obj)
        |> HeaderInternal.writeObject f

        (HttpHeaders.ifModifiedSince, preconditions.ifModifiedSince) |> HeaderInternal.writeDateTime f

        (HttpHeaders.ifMatch, preconditions.ifNoneMatch) 
        |> function
            | (header, Some (Choice1Of2 etags)) -> (header, etags :> obj)
            | (header, Some (Choice2Of2 any)) -> (header, any :> obj)
            | (header, _) -> (header, "" :> obj)
        |> HeaderInternal.writeObject f

        (HttpHeaders.ifUnmodifiedSince, preconditions.ifUnmodifiedSince) |> HeaderInternal.writeDateTime f

        (HttpHeaders.ifRange, preconditions.ifRange)
        |> function
            | (header, Some (Choice1Of2 etag)) -> (header, etag) |> HeaderInternal.writeObject f
            | (header, Some (Choice2Of2 date)) -> (header, Some date) |> HeaderInternal.writeDateTime f
            | _ -> ()
namespace FunctionalHttp.Interop

open FunctionalHttp
open System
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type ContentInfoExtensions private () = 
    [<Extension>]
    static member TryGetLength(this:ContentInfo, length : byref<int>) = 
        match this.Length with
        | None -> false
        | Some retval ->
            length <- retval
            true;

    [<Extension>]
    static member TryGetLocation(this:ContentInfo, uri : byref<Uri>) = 
        match this.Location with
        | None -> false
        | Some retval ->
            uri <- retval
            true;

    [<Extension>]
    static member TryGetMediaType(this:ContentInfo, mediaType : byref<MediaType>) = 
        match this.MediaType with
        | None -> false
        | Some retval ->
            mediaType <- retval
            true;
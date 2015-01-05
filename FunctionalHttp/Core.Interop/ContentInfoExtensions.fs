namespace FunctionalHttp.Core.Interop

open FunctionalHttp.Collections
open FunctionalHttp.Core
open System
open System.Runtime.CompilerServices

[<AbstractClass; Sealed; Extension>]
type ContentInfoExtensions private () = 
    [<Extension>]
    static member TryGetLength(this:ContentInfo, length : byref<uint64>) = 
        Option.tryGetValue this.Length &length

    [<Extension>]
    static member TryGetLocation(this:ContentInfo, uri : byref<Uri>) = 
        Option.tryGetValue this.Location &uri

    [<Extension>]
    static member TryGetMediaType(this:ContentInfo, mediaType : byref<MediaType>) = 
        Option.tryGetValue this.MediaType &mediaType
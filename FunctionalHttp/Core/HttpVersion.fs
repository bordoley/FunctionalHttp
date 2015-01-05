namespace FunctionalHttp.Core

open System
open System.Diagnostics.Contracts

[<Struct>]
type HttpVersion private (major:uint32, minor:uint32) =
    member this.Major = major
    member this.Minor = minor
    override this.ToString() = sprintf "HTTP/%u.%u" this.Major this.Minor
 
    static member Http1_1 = HttpVersion(1u, 1u)
    static member Http1_0 = HttpVersion(1u, 0u)
    static member Http0_9 = HttpVersion(0u, 9u)

    static member Create (major, minor) =
        if (major > 9u) then ArgumentOutOfRangeException "major" |> raise
        if (minor > 9u) then ArgumentOutOfRangeException "minor" |> raise 
        Contract.EndContractBlock();

        match (major, minor) with
        | (1u,1u) -> HttpVersion.Http1_1
        | (1u,0u) -> HttpVersion.Http1_0
        | (0u,9u) -> HttpVersion.Http0_9
        | _ -> HttpVersion(major, minor)

   

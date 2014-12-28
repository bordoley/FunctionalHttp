namespace FunctionalHttp.Core

open System
open System.Diagnostics.Contracts

[<Struct>]
type HttpVersion private (major:int, minor:int) =

    static member Http1_1 = HttpVersion(1, 1)
    static member Http1_0 = HttpVersion(1, 0)
    static member Http0_9 = HttpVersion(0, 9)

    static member Create (major, minor) =
        if (major < 0 || major > 9) then raise (ArgumentOutOfRangeException("major"))
        if (minor < 0 || minor > 9) then raise (ArgumentOutOfRangeException("major"))
        Contract.EndContractBlock();

        match (major, minor) with
        | (1,1) -> HttpVersion.Http1_1
        | (1,0) -> HttpVersion.Http1_0
        | (0,9) -> HttpVersion.Http0_9
        | _ -> HttpVersion(major, minor)

    member this.Major = major
    member this.Minor = minor

    override this.ToString() = sprintf "HTTP/%u.%u" this.Major this.Minor

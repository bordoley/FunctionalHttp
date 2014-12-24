namespace FunctionalHttp.Core

open System.Diagnostics.Contracts

type HttpVersion =
    private {
        major:int
        minor:int
    }

    static member Http1_1 = { major = 1; minor = 1; }
    static member Http1_0 = { major = 1; minor = 0; }
    static member Http0_9 = { major = 0; minor = 9; }

    static member Create (major, minor) =
        Contract.Requires(major >= 0 && major <= 9)
        Contract.Requires(minor >= 0 && minor <= 9)

        { major = major; minor = minor; }

    member this.Major = this.major
    member this.Minor = this.minor

    override this.ToString() = sprintf "HTTP/%u.%u" this.major this.minor

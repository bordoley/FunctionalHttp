namespace FunctionalHttp

type HttpVersion =
    private {
        major:int
        minor:int
    }

    static member Http1_1 = { major = 1; minor = 1; }
    static member Http1_0 = { major = 1; minor = 1; }

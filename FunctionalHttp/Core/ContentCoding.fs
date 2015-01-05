namespace FunctionalHttp.Core

type ContentCoding =
    private { contentCoding: string }

    static member Identity = { contentCoding = "identity" }
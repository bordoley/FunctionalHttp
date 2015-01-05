namespace FunctionalHttp.Core

type ContentCoding =
    private
    | ContentCoding

type Codings =
    private
    | ContentCoding of ContentCoding
    | Identity
    | Any

namespace FunctionalHttp.Core

type ContentCoding =
    | ContentCoding

type Codings =
    | ContentCoding of ContentCoding
    | Identity
    | Any

module Charset

open System.Text

type Charset =
    private 
        | Charset of string

    static member ANY = Charset "*"
    static member ISO_8859_1 = Charset "ISO-8859-1"
    static member US_ASCII  = Charset "US-ASCII"
    static member UTF_16 = Charset "UTF-16"
    static member UTF_16BE = Charset "UTF-16BE"
    static member UTF_16LE = Charset "UTF-16LE"
    static member UTF_8 = Charset "UTF-8"
    
    member this.Encoding 
        with get() =
            match this with
            | c when c = Charset.ISO_8859_1 -> Some <| Encoding.GetEncoding("iso-8859-1")
            | c when c = Charset.US_ASCII -> Some <| Encoding.GetEncoding("ascii")
            | c when c = Charset.UTF_8 -> Some Encoding.UTF8
            | _ -> None

    override this.ToString() = match this with | Charset charset -> charset

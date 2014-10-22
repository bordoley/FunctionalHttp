namespace FunctionalHttp

open System.Text

open HttpParsers

type Charset =
    private {
        charset:string
    }

    static member ANY = { charset = "*" }
    static member ISO_8859_1 = { charset = "ISO-8859-1" }
    static member US_ASCII  = { charset = "US-ASCII" }
    static member UTF_16 = { charset = "UTF-16" }
    static member UTF_16BE = { charset = "UTF-16BE" }
    static member UTF_16LE = { charset = "UTF-16LE" }
    static member UTF_8 = { charset =  "UTF-8" }

    static member internal Parser : Parser<char, Charset> = 
        token |> Parser.map (fun parsed -> 
            match parsed.ToUpperInvariant() with
            | x when x = Charset.UTF_8.ToString() -> Charset.UTF_8
            | x when x = Charset.ISO_8859_1.ToString() -> Charset.ISO_8859_1
            | x when x = Charset.US_ASCII.ToString() -> Charset.US_ASCII
            | x when x = Charset.ANY.ToString() -> Charset.ANY
            | x -> { charset = x })

    member this.Encoding 
        with get() =
            match this with
            | c when c = Charset.ISO_8859_1 -> Some <| Encoding.GetEncoding("iso-8859-1")
            | c when c = Charset.US_ASCII -> Some <| Encoding.GetEncoding("ascii")
            | c when c = Charset.UTF_8 -> Some Encoding.UTF8
            | _ -> None

    override this.ToString() = this.charset

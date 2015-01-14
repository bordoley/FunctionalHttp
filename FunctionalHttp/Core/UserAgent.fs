namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System.Text

open HttpParsers

type UserAgent = 
    private { product:Product; additional:Choice<Product,Comment> list }

    override this.ToString() =  
        string this.product + " " +
        (this.additional 
            |> Seq.map (function
                | Choice1Of2 product -> string product
                | Choice2Of2 comment -> string comment)
            |> String.concat " ")

    static member internal Parser =
        let additional = many (RWS >>. (Product.Parser <^> Comment.Parser))
        Product.Parser .>>. additional |>> fun (product, additional) -> 
            { product = product; additional = List.ofSeq additional }

    static member Create ua =
        match parse UserAgent.Parser ua with
        | Success (ua, _) -> ua
        | _ -> invalidArg "ua" "Not a valid User-Agent string"
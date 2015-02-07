namespace FunctionalHttp.Core

open Sparse
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

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal UserAgent = 
    let internal parser : Parser<UserAgent> = 
        let additional = many (RWS >>. (Product.parser <^> Comment.parser))
        Product.parser .>>. additional |>> fun (product, additional) -> 
            { product = product; additional = List.ofSeq additional }

type UserAgent with  
    static member Create ua =
        match parse UserAgent.parser ua with
        | Success (ua, _) -> ua
        | _ -> invalidArg "ua" "Not a valid User-Agent string"


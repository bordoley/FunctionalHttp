namespace FunctionalHttp.Core

open Sparse
open System.Text

open HttpCharMatchers

type internal Comment = 
    private { parts : Choice<string,Comment> list }

    override this.ToString() =  
        "(" +
        (this.parts 
        |> Seq.map (function
            | Choice1Of2 commentText -> Comment.EncodeCommentText commentText
            | Choice2Of2 comment -> string comment)
        |> String.concat " ") + ")"

    static member private EncodeCommentText (text:string) = 
        let builder:StringBuilder ref = ref null

        for i = 0 to text.Length - 1 do
            match text.Chars 0 with
            | c when ctext c ->     
                if (!builder) <> null then (!builder).Append c |> ignore
            | c ->
                if (!builder) <> null then builder := StringBuilder().Append(text, 0, i)
                (!builder).Append('\\').Append(c) |> ignore

        if (!builder) <> null 
            then (!builder).ToString()
        else text

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Comment = 
    let parser =
        let ESCAPE_CHAR = '\\';

        let comment_text (input:CharStream) = 
            let builder:StringBuilder ref = ref null
            let Eof = Fail input.Length

            let rec doParse index =
                if index = input.Length
                    then Eof
                else 
                    match input.Item index with
                    | c when c = ESCAPE_CHAR -> 
                        if !builder = null 
                            then builder := StringBuilder(input.ToString(0, index))

                        match index + 1 with
                        | index when index = input.Length -> Eof
                        | index when quoted_pair_char (input.Item index) ->
                            (!builder).Append(input.Item index) |> ignore
                            doParse (index + 1)      
                        | index -> Fail index
                    | c when ctext c ->
                        if !builder <> null then (!builder).Append(c) |> ignore
                        doParse (index + 1)      
                    | _ -> 
                        if index = 0 then Fail 0
                        else if !builder = null 
                            then Success(input.ToString(0, index), index)
                        else Success(builder.ToString(), index)
            doParse 0
        
        let (comment_segment, comment_segment_impl)  = createParserForwardedToRef ()

        let comment =
            pOpenParen >>. (many comment_segment) .>> pCloseParen 
            |>> (fun segments -> { parts = List.ofSeq segments })

        comment_segment_impl := comment_text <^> comment
         
        comment

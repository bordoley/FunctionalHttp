namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System.Text

open FunctionalHttp.Parsing.CharMatchers
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Core.HttpCharMatchers

type Comment = 
    private { parts : Choice<string,Comment> list }

    override this.ToString() =  
        "(" +
        (this.parts 
        |> Seq.map (function
            | Choice1Of2 commentText -> Comment.EncodeCommentText commentText
            | Choice2Of2 comment -> string comment)
        |> String.concat " ") + ")"

    static member internal Parser =
        let ESCAPE_CHAR = '\\';

        let comment_text (input:CharStream) = 
            let builder:StringBuilder ref = ref null

            let rec doParse index =
                if index = input.Length
                    then Eof
                else 
                    match input.Item index with
                    | c when c = ESCAPE_CHAR -> 
                        if !builder = null 
                            then builder := StringBuilder(input.SubSequence(0, index).ToString())

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
                            then Success(input.SubSequence(0, index).ToString(), index, input.SubSequence(index))
                        else Success(builder.ToString(), index, input.SubSequence(index))
            doParse 0
        
        let (comment_segment, comment_segment_impl)  = createParserForwardedToRef ()

        let comment =
            (pchar '(') >>. (many comment_segment) .>> (pchar ')') 
            |>> (fun segments -> { parts = List.ofSeq segments })

        comment_segment_impl := comment_text <^> comment
         
        comment

    static member private EncodeCommentText (text:string) = 
        let builder:StringBuilder ref = ref null

        for i = 0 to text.Length - 1 do
            match text.Chars 0 with
            | c when HttpCharMatchers.ctext c ->     
                if (!builder) <> null then (!builder).Append c |> ignore
            | c ->
                if (!builder) <> null then builder := StringBuilder().Append(text, 0, i)
                (!builder).Append('\\').Append(c) |> ignore

        if (!builder) <> null 
            then (!builder).ToString()
        else text
namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System.Text

open FunctionalHttp.Parsing.CharMatchers
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Core.HttpCharMatchers

type Comment =
    private | Comment of list<Choice<string,Comment>>

    override this.ToString() =  
        let builder:StringBuilder = StringBuilder()
        
        builder.Append('(') |> ignore

        match this with 
        | Comment parts -> 
            parts
            |> Seq.iter (fun e ->
                let value = 
                    match e with 
                    | Choice1Of2 commentText -> Comment.EncodeCommentText commentText
                    | Choice2Of2 comment -> comment.ToString()
 
                builder.Append value |> ignore)

        builder.Append(')').ToString()

    static member internal Parser =
        let ESCAPE_CHAR = '\\';

        let comment_text (input:CharStream) = 
            let builder:StringBuilder ref = ref null

            let rec doParse index =
                if index = input.Length
                    then Eof input
                else 
                    match input.Item index with
                    | c when c = ESCAPE_CHAR -> 
                        if !builder = null 
                            then builder := StringBuilder(input.SubSequence(0, index).ToString())

                        match index + 1 with
                        | index when index = input.Length -> Eof input
                        | index when ctext (input.Item index) ->
                            (!builder).Append(input.Item index) |> ignore
                            doParse (index+1)      
                        | _ -> Fail input
                    | c when ctext c ->
                        if !builder <> null then (!builder).Append(c) |> ignore
                        doParse (index + 1)      
                    | _ -> 
                        if !builder = null 
                            then Success(input.SubSequence(0, index).ToString(), input.SubSequence(index))
                        else Success(builder.ToString(), input.SubSequence(index))
    
            if (input.Length = 0)
                then Eof input
            else doParse 0
        
        let (comment_segment, comment_segment_impl)  = createParserForwardedToRef ()

        let comment =
            (pchar '(') .>>. (many comment_segment) .>>. (pchar ')') 
            |>> (fun ((_, segments),_) -> Comment (List.ofSeq segments))

        comment_segment_impl := comment_text <^> comment
         
        comment

    static member private EncodeCommentText (text:string) = 
        let builder:StringBuilder ref = ref null

        for i = 0 to text.Length do
            match text.Chars 0 with
            | c when HttpCharMatchers.ctext c ->     
                if (!builder) <> null then (!builder).Append c |> ignore
            | c ->
                if (!builder) <> null then builder := StringBuilder().Append(text, 0, i)
                (!builder).Append('\\').Append(c) |> ignore

        if (!builder) <> null 
            then (!builder).ToString()
        else text
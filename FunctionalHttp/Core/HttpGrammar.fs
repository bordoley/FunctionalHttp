namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System.Text

open FunctionalHttp.Parsing.CharMatchers

module internal HttpCharMatchers = 
    let tchar = ALPHA_NUMERIC <||> anyOf "!#\$%&'*+-.^_`|~"
    let obs_text = inRange (char 0x80) (char 0xFF)
    let qdtext = HTAB <||> SP <||> is (char 0x21) <||> inRange (char 0x23) (char 0x5B) <||> inRange (char 0x5D) (char 0x7E) <||> obs_text
    let quoted_pair_char = (HTAB <||> SP <||> VCHAR <||> obs_text)

    let ctext = 
        HTAB <||> 
        SP <||> 
        inRange (char 0x21) (char 0x27) <||> 
        inRange (char 0x2A) (char 0x5B) <||> 
        inRange (char 0x5D) (char 0x7E) <||> 
        obs_text

open HttpCharMatchers
open FunctionalHttp.Parsing.Parser

module internal HttpParsers =
    let OWS : Parser<string> = CharMatchers.many WSP

    let BWS = OWS
    
    let RWS : Parser<string> = CharMatchers.many1 WSP

    let OWS_SEMICOLON_OWS : Parser<string> = OWS .>>. (pchar ';') .>>. OWS |>> (fun _ -> ";");
    
    let OWS_COMMA_OWS : Parser<string> = OWS .>>. (pchar ',') .>>. OWS |>> (fun _ -> ",");

    let token : Parser<string> = CharMatchers.many1 tchar

    let token68 : Parser<string> = CharMatchers.many1(ALPHA <||> DIGIT <||> (anyOf "-._~+/" )) .>>. (CharMatchers.many EQUALS) |>> (fun (a,b) -> a + b)

    let private DQUOTE_CHAR = (char 34)
    let private ESCAPE_CHAR = '\\';

    let quoted_string (input:CharStream) = 
        let builder:StringBuilder ref = ref null

        let rec doParse index =
            if index = input.Length
                then Eof
            else 
                match input.Item index with
                | c when c = ESCAPE_CHAR -> 
                    if !builder = null 
                        then builder := StringBuilder(input.SubSequence(1, index - 1).ToString())

                    match index + 1 with
                    | index when index = input.Length -> Eof
                    | index when quoted_pair_char (input.Item index) ->
                        (!builder).Append(input.Item index) |> ignore
                        doParse (index+1)      
                    | index -> Fail index
                | c when c = DQUOTE_CHAR -> 
                    match !builder with
                    | null -> Success(input.SubSequence(1, index - 1).ToString(), index + 1, input.SubSequence(index + 1))
                    | builder -> Success(builder.ToString(), index + 1, input.SubSequence(index + 1))
                | c when qdtext c ->
                    if !builder <> null then (!builder).Append(c) |> ignore
                    doParse (index + 1)      
                | _ -> Fail index
    
        if (input.Length = 0)
            then Eof
        else if (input.Item 0) <> DQUOTE_CHAR
            then Fail 0
        else doParse 1

    let httpList p =
        p |> sepBy OWS_COMMA_OWS

module internal HttpEncoding =
    let private DQUOTE_CHAR = (char 34)
    let private ESCAPE_CHAR = '\\';

    let asQuotedString (input:string) =
        let retval = StringBuilder()
        retval.Append DQUOTE_CHAR |> ignore

        for i = 0 to input.Length - 1 do 
            let c = input.Chars i
            if qdtext c
                then retval.Append c |> ignore
            else if (c = DQUOTE_CHAR || c = ESCAPE_CHAR)
                then retval.Append(ESCAPE_CHAR).Append(c) |> ignore
            else failwith (sprintf "The character %c cannot be included in a quoted string" c)

        retval.Append DQUOTE_CHAR |> ignore
        retval.ToString()

    let asTokenOrQuotedString (input:string) =
        match parse HttpParsers.token input with
        | Some result -> result
        | None ->  asQuotedString input

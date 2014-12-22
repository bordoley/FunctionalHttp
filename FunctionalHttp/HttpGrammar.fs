namespace FunctionalHttp

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
        inRange (char 0x21) (char 0x5B) <||> 
        inRange (char 0x2A) (char 0x5B) <||> 
        inRange (char 0x5D) (char 0x7E) <||> 
        obs_text

open HttpCharMatchers
open FunctionalHttp.Parsing.Parser

module internal HttpParsers =
    let OWS : Parser<string> = CharMatchers.many WSP

    let BWS = OWS
    
    let RWS : Parser<string> = CharMatchers.many1 WSP

    let OWS_SEMICOLON_OWS : Parser<string> = OWS <+> (parseChar ';') <+> OWS |> map (fun _ -> ";");
    
    let OWS_COMMA_OWS : Parser<string> = OWS <+> (parseChar ',') <+> OWS |> map (fun _ -> ",");

    let token : Parser<string> = CharMatchers.many1 tchar

    let token68 : Parser<string> = CharMatchers.many1(ALPHA <||> DIGIT <||> (anyOf "-._~+/" )) <+> (CharMatchers.many EQUALS) |> map (fun (a,b) -> a + b)

    let private DQUOTE_CHAR = (char 22)
    let private ESCAPE_CHAR = '\\';

    let quoted_string (input:CharStream) = 
        let builder:StringBuilder ref = ref null

        let rec doParse index =
            if index = input.Length
                then Eof input
            else 
                match input.Item index with
                | c when c = ESCAPE_CHAR -> 
                    if !builder = null 
                        then builder := StringBuilder(input.SubSequence(1, index - 1).ToString())

                    match index + 1 with
                    | index when index = input.Length -> Eof input
                    | index when quoted_pair_char (input.Item index) ->
                        (!builder).Append(input.Item index) |> ignore
                        doParse (index+1)      
                    | _ -> Fail input
                | c when c = DQUOTE_CHAR -> 
                    match !builder with
                    | null -> Success(input.SubSequence(1, index - 1).ToString(), input.SubSequence(index + 1))
                    | builder -> Success(builder.ToString(), input.SubSequence(index+1))
                | c when qdtext c ->
                    if !builder <> null then (!builder).Append(c) |> ignore
                    doParse (index + 1)      
                | _ -> Fail input
    
        if (input.Length = 0)
            then Eof input
        else if (input.Item 0) <> DQUOTE_CHAR
            then Fail input
        else doParse 1

module internal HttpEncoding =
    let private DQUOTE_CHAR = (char 22)
    let private ESCAPE_CHAR = '\\';

    let asQuotedString (input:string) =
        let retval = StringBuilder()

        for i = 0 to input.Length do 
            let c = input.Chars 0
            if qdtext c
                then retval.Append c |> ignore
            else if (c = DQUOTE_CHAR || c = ESCAPE_CHAR)
                then retval.Append(ESCAPE_CHAR).Append(c) |> ignore
            else failwith (sprintf "The character %c cannot be included in a quoted string" c)
        retval.ToString()

    let asTokenOrQuotedString (input:string) =
        match Parser.parse HttpParsers.token input with
        | Some result -> result
        | None -> asQuotedString input

namespace FunctionalHttp

open System.Text

open CharMatchers

module internal HttpCharMatchers = 
    let tchar = ALPHA_NUMERIC <||> anyOf "!#\$%&'*+-.^_`|~"
    let obs_text = inRange (char 0x80) (char 0xFF)
    let qdtext = HTAB <||> SP <||> is (char 0x21) <||> inRange (char 0x23) (char 0x5B) <||>  inRange (char 0x5D) (char 0x7E) <||> obs_text
    let quoted_pair_char = (HTAB <||> SP <||> VCHAR <||> obs_text)

open HttpCharMatchers
open Parser

module internal HttpParsers =
    let OWS : Parser<char,string> = CharMatchers.many WSP

    let BWS = OWS
    
    let RWS : Parser<char,string> = CharMatchers.many1 WSP

    let OWS_SEMICOLON_OWS : Parser<char,string> = OWS <+> (matches SEMICOLON) <+> OWS |> map (fun _ -> ";");
    
    let token : Parser<char,string> = CharMatchers.many1 tchar

    let token68 : Parser<char,string> = CharMatchers.many1(ALPHA <||> DIGIT <||>  (anyOf "-._~+/" )) <+> (CharMatchers.many EQUALS) |> map (fun (a,b) -> a + b)

    let private DQUOTE_CHAR = '"'
    let private ESCAPE_CHAR = '\\';

    let quoted_string (input:IInput<char>) =    
        let retval: IParseResult<char, string> option ref = ref None
        let builder:Option<StringBuilder> ref = ref Option.None

        let endIndex = ref 0
        let index = ref 0

        if (input.Length = 0 || (input.Item !index) <> DQUOTE_CHAR)
        then retval:= Some (Fail input)
        else 
            index := 1
            while ((!index < input.Length) && (Option.isNone !retval))
                do 
                    let c = input.Item !index
                    if (qdtext c) 
                    then match !builder with
                            | None -> endIndex := !endIndex + 1
                            | Some x -> x.Append(c) |> ignore
                    else if c = ESCAPE_CHAR
                    then 
                        let strBuild =
                            match !builder with
                            | Some builder -> builder
                            | _ ->
                                builder := Some (StringBuilder(input.SubSequence(0,!endIndex).ToString()))
                                (!builder).Value

                        index := !index + 1

                        if not (!index < input.Length)
                        then retval := Some (Eof input)
                        else if (quoted_pair_char (input.Item !index))
                        then strBuild.Append(input.Item !index) |> ignore
                        else retval := Some (Fail input) 
                    else if c = DQUOTE_CHAR
                    then
                        endIndex := !endIndex + 1
                        match !builder with
                        | Some buf -> 
                            retval := Some (Success(buf.ToString(), input.SubSequence(!endIndex + 1)))
                        | None -> 
                            retval := Some (Success(input.SubSequence(2, !endIndex + 1).ToString(), input.SubSequence(!endIndex + 1)))
                    else 
                        retval := Some (Fail input)

                    index := !index + 1
        match !retval with
        | None -> Eof input
        | Some r -> r

namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System
open System.Text

open FunctionalHttp.Parsing.CharMatchers

module internal Predicates =
    let is (arg:'T) (c:'T) = 
        c = arg

    let (<&&>) m1 m2 (c:'T) = (m1 c) && (m2 c)

    let (<||>) m1 m2 (c:'T) = (m1 c) || (m2 c)

module internal CharConstants = 
    open CharMatchers
    open Predicates

    let AMPERSAND = is '&'

    let ASTERISK = is '*'

    let BACK_SLASH = is '\\'

    let CLOSE_PAREN = is ')'

    let COLON = is ':'

    let COMMA = is ','

    let CR = is (char 13)

    let EQUALS = is '='

    let DASH = is '-'

    let DQUOTE = is (char 34)

    let FORWARD_SLASH = '/'

    let HTAB = is (char 9)

    let LF = is (char 10)

    let OPEN_PAREN = is '('

    let PERIOD = is '.'

    let POUND_SIGN = is '#'

    let QUESTION_MARK = is '?'

    let SEMICOLON = is ';'

    let SP = is ' '

// See http://tools.ietf.org/html/rfc5234#appendix-B.1
module internal Abnf = 
    open CharMatchers
    open Predicates

    let ALPHA = (inRange 'a' 'z') <||> (inRange 'A' 'Z')

    let DIGIT = inRange '0' '9'

    let ALPHA_NUMERIC = ALPHA <||> DIGIT

    let BIT = inRange '0' '1'

    let CHAR = inRange (char 1) (char 127)

    let CTL = (inRange (char 0) (char 0x1F)) <||> (is (char 0x7F))

    let OCTEXT = inRange (char 0) (char 0xFF)

    let HEXDIG = (inRange '0' '9') <||> (inRange 'A' 'F')

    let VCHAR = inRange (char 0x21) (char 0x7E)

    open CharConstants
    let WSP = SP <||> HTAB

module internal HttpCharMatchers = 
    open Abnf
    open CharConstants
    open Predicates

    let tchar = ALPHA_NUMERIC <||> isAnyOf "!#\$%&'*+-.^_`|~"
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

module internal HttpParsers =
    open HttpCharMatchers
    open FunctionalHttp.Parsing.Parser
    open FunctionalHttp.Parsing.CharParsers
    open Abnf
    open CharConstants
    open Predicates

    let OWS : Parser<string> = manySatisfy WSP

    let BWS = OWS
    
    let RWS : Parser<string> = many1Satisfy WSP

    let OWS_SEMICOLON_OWS : Parser<string> = OWS .>>. (pchar ';') .>>. OWS |>> (fun _ -> ";");
    
    let OWS_COMMA_OWS : Parser<string> = OWS .>>. (pchar ',') .>>. OWS |>> (fun _ -> ",");

    let token : Parser<string> = many1Satisfy tchar

    let token68 : Parser<string> = many1Satisfy(ALPHA <||> DIGIT <||> (isAnyOf "-._~+/" )) .>>. (manySatisfy EQUALS) |>> (fun (a,b) -> a + b)

    let private DQUOTE_CHAR = (char 34)
    let private ESCAPE_CHAR = '\\';

    let quoted_string (input:CharStream) = 
        let builder:StringBuilder ref = ref null
        let Eof = Fail input.Length

        let rec doParse index =
            if index = input.Length
                then Eof
            else 
                match input.Item index with
                | c when c = ESCAPE_CHAR -> 
                    if !builder = null 
                        then builder := StringBuilder(input.ToString(1, index + 1))
                       
                    match index + 1 with
                    | index when index = input.Length -> Eof
                    | index when quoted_pair_char input.[index] ->
                        (!builder).Append(input.Item index) |> ignore
                        doParse (index + 1)      
                    | index -> Fail index
                | c when c = DQUOTE_CHAR -> 
                    match !builder with
                    | null -> Success(input.ToString(1, index - 1), index + 1)
                    | builder -> Success(builder.ToString(), index + 1)
                | c when qdtext c ->
                    if !builder <> null then (!builder).Append(c) |> ignore
                    doParse (index + 1)      
                | _ -> Fail index
        
        if input.Length = 0 then Eof
        else if input.[0] <> DQUOTE_CHAR then Fail 0
        else doParse 1

    let httpList p =
        sepBy p OWS_COMMA_OWS

    let httpList1 p =
        sepBy1 p OWS_COMMA_OWS

    let httpDate:Parser<DateTime> =
        let day_name = 
            ((pstring "Mon") |>> fun _ -> DayOfWeek.Monday)    <|>
            ((pstring "Tue") |>> fun _ -> DayOfWeek.Tuesday)   <|>
            ((pstring "Wed") |>> fun _ -> DayOfWeek.Wednesday) <|>
            ((pstring "Thu") |>> fun _ -> DayOfWeek.Thursday)  <|>
            ((pstring "Fri") |>> fun _ -> DayOfWeek.Friday)    <|>
            ((pstring "Sat") |>> fun _ -> DayOfWeek.Saturday)  <|>
            ((pstring "Sun") |>> fun _ -> DayOfWeek.Sunday) 
        
        // FIXME: Int32.Parse can throw        
        let day = manyMinMaxSatisfy 2 2 DIGIT |>> fun x -> Int32.Parse x
        let month =
            ((pstring "Jan") |>> fun _ -> 1)  <|>
            ((pstring "Feb") |>> fun _ -> 2)  <|>
            ((pstring "Mar") |>> fun _ -> 3)  <|>
            ((pstring "Apr") |>> fun _ -> 4)  <|>
            ((pstring "May") |>> fun _ -> 5)  <|>
            ((pstring "Jun") |>> fun _ -> 6)  <|>
            ((pstring "Jul") |>> fun _ -> 7)  <|>
            ((pstring "Aug") |>> fun _ -> 8)  <|>
            ((pstring "Sep") |>> fun _ -> 9)  <|>
            ((pstring "Oct") |>> fun _ -> 10) <|>
            ((pstring "Nov") |>> fun _ -> 11) <|>
            ((pstring "Dec") |>> fun _ -> 12)

        // FIXME: Int32.Parse can throw       
        let year = manyMinMaxSatisfy 4 4 DIGIT |>> fun x -> Int32.Parse x
           
        let date1 =  day .>> pchar ' ' .>>. month .>> pchar ' ' .>>. year

        let hour = manyMinMaxSatisfy 2 2 DIGIT |>> fun x -> Int32.Parse x
        let minute = manyMinMaxSatisfy 2 2 DIGIT |>> fun x -> Int32.Parse x
        let second = manyMinMaxSatisfy 2 2 DIGIT |>> fun x -> Int32.Parse x

        let timeOfDay = hour .>>  pchar ':' .>>. minute .>> pchar ':' .>>. second

        let imf_fix_date = 
            day_name .>>. pstring ", " >>. date1 .>> pchar ' ' .>>. timeOfDay .>> pstring " GMT" 
            |>> fun (((day, month),year), ((hour, minute), second)) ->
                DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc)

        // FIXME: Need to implement obs-date = rfc850-date / asctime-date
        // See: https://tools.ietf.org/html/rfc7231
        imf_fix_date

module internal HttpEncoding =
    open HttpCharMatchers
    open FunctionalHttp.Parsing.Parser

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
        | Success (result, _) -> result
        | _->  asQuotedString input

    let dateToHttpDate (date:DateTime) =
        let date = date.ToUniversalTime()
        let dayOfWeek =
            match date.DayOfWeek with
            | DayOfWeek.Monday -> "Mon"
            | DayOfWeek.Tuesday -> "Tue"
            | DayOfWeek.Wednesday -> "Wed"
            | DayOfWeek.Thursday -> "Thu"
            | DayOfWeek.Friday -> "Fri"
            | DayOfWeek.Saturday -> "Sat"
            | DayOfWeek.Sunday -> "Sun"
            | _ -> failwith "Invalid day of the week."

        let day = date.Day.ToString("00")
        let month = 
            match date.Month with
            | 1 ->  "Jan"
            | 2 ->  "Feb"
            | 3 ->  "Mar"
            | 4 ->  "Apr"
            | 5 ->  "May"
            | 6 ->  "Jun"
            | 7 ->  "Jul"
            | 8 ->  "Aug"
            | 9 ->  "Sep"
            | 10 -> "Oct"
            | 11 -> "Nov"
            | 12 -> "Dec"
            | _ -> failwith "Invalid month of the year."

        let year  = date.Year.ToString("0000")

        let hour = date.Hour.ToString("00")
        let minute = date.Minute.ToString("00")
        let second = date.Second.ToString("00")


        let imf_fixdate = dayOfWeek + ", " + day + " " + month + " " + year + " " + hour + ":" + minute + ":" + second + " GMT" 

        imf_fixdate
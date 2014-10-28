namespace FunctionalHttp

open System

type internal CharMatcher = char -> bool

module internal CharMatchers =
    let any (c:char) = true

    let anyOf (chars:string) =
        let chars = chars.ToCharArray()
        Array.sortInPlace chars
        fun c -> Array.BinarySearch(chars, c) > 0

    let inRange (start:char) (last:char) (c:char) = c >= start && c <= last
    
    let is (arg:char) (c:char) = c = arg

    let many (matcher:CharMatcher) (input: IInput<char>) =
        let rec findLast index =
            if index = input.Length
            then index
            else if matcher (input.Item index)
            then findLast (index + 1)
            else index

        let result = findLast 0

        Success ((input.SubSequence(0, result).ToString()), input.SubSequence(result))

    let many1 (matcher:CharMatcher) (input: IInput<char>) =
        let result = many matcher input
        match result with
        | Success (value, next) -> 
            if value.Length = 0 then Fail input else result
        | _ -> result

    let none (c:char) = false

    let notMatch (matcher:CharMatcher) (c:char) = not (matcher c)

    let (<&&>) (m1:CharMatcher) (m2:CharMatcher) (c:char) = m1(c) && m2(c)

    let (<||>) (m1:CharMatcher) (m2:CharMatcher) (c:char) = m1(c) || m2(c)

    // See http://tools.ietf.org/html/rfc5234#appendix-B.1
    let ALPHA = (inRange 'a' 'z') <||> (inRange 'A' 'Z')

    let DIGIT = inRange '0' '9'

    let ALPHA_NUMERIC = ALPHA <||> DIGIT

    let AMPERSAND = is '&'

    let ASTERISK = is '*'

    let BACK_SLASH = is '\\'

    let BIT = inRange '0' '1'

    let CHAR = inRange (char 1) (char 127)

    let CLOSE_PAREN = is ')'

    let COLON = is ':'

    let COMMA = is ','

    let CR = is (char 13)

    let CTL = (inRange (char 0) (char 0x1F)) <||> (is (char 0x7F))

    let EQUALS = is '='

    let DASH = is '-'

    let DQUOTE = is '"'

    let FORWARD_SLASH = is '/'

    // HEXDIG as defined in HTTP. Major difference from standard ABNF is that alpha characters are case insensitive
    let HEXDIG = (inRange '0' '9') <||> (inRange 'a' 'f')

    let HTAB = is (char 9)

    let LF = is (char 10)

    let OCTEXT = inRange (char 0) (char 0xFF)

    let OPEN_PAREN = is '('

    let PERIOD = is '.'

    let POUND_SIGN = is '#'

    let QUESTION_MARK = is '?'

    let SEMICOLON = is ';'

    let SP = is ' '

    let VCHAR = inRange (char 0x21) (char 0x7E)

    let WSP = SP <||> HTAB



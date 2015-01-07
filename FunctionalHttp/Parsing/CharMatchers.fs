namespace FunctionalHttp.Parsing

open System

type internal CharMatcher = char -> bool

module internal CharMatchers =
    let isAnyOf (chars:string) =
        // FIXME: Probably add some heuristics here based upon the number of chars in the string
        // and switch the implementation
        let chars = chars.ToCharArray()
        Array.sortInPlace chars
        fun c -> Array.BinarySearch(chars, c) > 0

    let inRange (start:char) (last:char) (c:char) = 
        c >= start && c <= last

// FIXME: These are so generic that it seems they should be in terms of 'T
    let is (arg:char) (c:char) = 
        c = arg

    let (<&&>) (m1:CharMatcher) (m2:CharMatcher) (c:char) = m1(c) && m2(c)

    let (<||>) (m1:CharMatcher) (m2:CharMatcher) (c:char) = m1(c) || m2(c)

module internal CharParsers =
    let manySatisfy (matcher:CharMatcher) (input: CharStream) =
        let rec findLast index =
            if index = input.Length
            then index
            else if matcher (input.Item index)
            then findLast (index + 1)
            else index

        let result = findLast 0

        Success ((input.SubSequence(0, result).ToString()), result)

    let many1Satisfy (matcher:CharMatcher) (input: CharStream) =
        let result = manySatisfy matcher input
        match result with
        | Success (value, next) -> 
            if value.Length = 0 then Fail 0 else result
        | _ -> result

module internal CharConstants = 
    open CharMatchers

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


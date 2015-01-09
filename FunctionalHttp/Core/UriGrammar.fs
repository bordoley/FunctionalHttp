namespace FunctionalHttp.Core
open System
open FunctionalHttp.Parsing
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Parsing.CharMatchers
open FunctionalHttp.Parsing.CharParsers

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

module internal UriGrammar = 
    open Abnf
    open Predicates

    let unreserved = ALPHA <||> DIGIT <||> isAnyOf "-._~"
    let subdelims = isAnyOf "!$&'()*+,;="
    let pctencoded : Parser<string> = pchar '%' >>. manyMinMaxSatisfy 2 2 HEXDIG |>> fun x -> "%" + x
    let regname : Parser<string> = 
        (manySatisfy unreserved <|> pctencoded <|> manySatisfy subdelims) |> many |>> String.concat ""
    // Fixme: host  = IP-literal / IPv4address / reg-name
    // See: https://tools.ietf.org/html/rfc3986#section-3.2.2
    let host = regname

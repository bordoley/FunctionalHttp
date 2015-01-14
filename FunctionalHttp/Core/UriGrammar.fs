namespace FunctionalHttp.Core
open System
open FunctionalHttp.Parsing

module internal Predicates =
    let is (arg:'T) (c:'T) = 
        c = arg

    let (<||>) m1 m2 (c:'T) = (m1 c) || (m2 c)

// See http://tools.ietf.org/html/rfc5234#appendix-B.1
module internal Abnf = 
    open CharMatchers
    open Predicates

    let ALPHA_NUMERIC = (inRange 'a' 'z') <||> (inRange 'A' 'Z') <||> isDigit

    let CHAR = inRange (char 1) (char 127)

    let HEXDIG = (inRange '0' '9') <||> (inRange 'A' 'F')

    let VCHAR = inRange (char 0x21) (char 0x7E)

    let SP = is ' '
    let HTAB = is (char 9)
    let WSP = SP <||> HTAB

module internal CharParsers =
    let pSemicolon : Parser<char> = pchar ';'
    let pComma : Parser<char> = pchar ','
    let pSpace : Parser<char> = pchar ' '
    let pColon : Parser<char> = pchar ':'
    let pPeriod : Parser<char> = pchar '.' 
    let pEquals : Parser<char> = pchar '=' 
    let pForwardSlash : Parser<char> = pchar '/'
    let pDash : Parser<char> = pchar '-'
    let pOpenParen : Parser<char> = pchar '('
    let pCloseParen : Parser<char> = pchar ')'
    let pQuote : Parser<char> = pchar (char 34)
    let pAsterisk : Parser<char> = pchar '*'
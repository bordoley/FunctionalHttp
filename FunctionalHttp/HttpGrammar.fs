namespace FunctionalHttp

open CharMatchers

module internal HttpCharMatchers = 
    let tchar = ALPHA_NUMERIC <||> anyOf "!#\$%&'*+-.^_`|~"

open HttpCharMatchers
open Parser

module internal HttpParsers =
    let OWS : Parser<char,string> = CharMatchers.many WSP

    let BWS = OWS
    
    let RWS : Parser<char,string> = CharMatchers.many1 WSP
    
    let token : Parser<char,string> = CharMatchers.many1 tchar

    let token68 : Parser<char,string> = CharMatchers.many1(ALPHA <||> DIGIT <||>  (anyOf "-._~+/" )) <+> (CharMatchers.many EQUALS) |> map (fun (a,b) -> a + b)

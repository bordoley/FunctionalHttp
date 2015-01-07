namespace FunctionalHttp.Parsing

open System;
open System.Collections.Generic

type internal IParseResult<'TResult> =
    | Success of  result : 'TResult * next : int
    | Fail of iFailed : int

type internal Parser<'TResult> = CharStream -> IParseResult<'TResult>

module internal Parser =         
    // map
    let (|>>) (p:Parser<_>) f (input:CharStream) =
        let result = p input
        match result with
        | Fail i -> Fail i
        | Success (result, next) -> Success (f result, next)

    let (.>>.) (p1:Parser<'T1>) (p2:Parser<'T2>) (input:CharStream) = 
        match p1 input with
        | Fail i -> Fail i
        | Success (result1, next) -> 
            match input.SubSequence(next) |> p2 with
            | Fail next2 -> Fail (next + next2)
            | Success (result2, next2) -> Success ((result1, result2), next + next2)
     
    let (>>.) (p1:Parser<_>) (p2:Parser<_>) = 
        p1 .>>. p2 |>> fun (r1,r2) -> r2

    let (.>>) (p1:Parser<_>) (p2:Parser<_>) = 
        p1 .>>. p2 |>> fun (r1,r2) -> r1

    // NOTE: Doesn't apper to be a corresponding FParsec combinator to this
    let (<^>) (p1:Parser<_>) (p2:Parser<_>) (input:CharStream) = 
        match p1 input with 
        | Success (result, next) -> 
            Success (Choice1Of2 result, next)
        | _ -> 
            match p2 input with
            | Success (result, next) -> Success (Choice2Of2 result, next)
            | Fail i -> Fail i

    let (<|>) (p1:Parser<_>) (p2:Parser<_>) = 
        let p = p1 <^> p2
        fun (input:CharStream) ->
            match p input with
            | Fail i -> Fail i
            | Success (Choice1Of2 result, next) -> Success (result, next)
            | Success (Choice2Of2 result, next) -> Success (result, next)

    let eof (input:CharStream) =
        if input.Length = 0 
        then Success ((), 0)
        else Fail 0

    let followedBy (pnext:Parser<_>) (input:CharStream) =
        match pnext input with
        | Success _ -> Success ((), 0)
        | Fail i -> Fail i

    let createParserForwardedToRef () =
        let dummy (input:CharStream) = failwith "a parser created with createParserForwardedToRef was not initialized"
        let r = ref dummy
        (fun input -> !r input), r : Parser<_> * Parser<_> ref

    // FIXME: Don't cheat using ref cells
    let many (p:Parser<_>) (input:CharStream) =
        let remainder = ref input
        let index = ref 0

        let rec doParse input =
            let result = p input

            match result with
            | Success (result, next) -> 
                index := !index + next
                result::(input.SubSequence(next) |> doParse)
            | Fail i-> 
                remainder := input
                []
        
        let result = doParse input :> _ seq
        Success (result, !index)
     
    let many1 (p:Parser<_>) (input:CharStream) =   
       match many p input with
       | Fail i -> Fail i
       | Success (result, next) as success ->
           if Seq.isEmpty result
           then Fail 0
           else success

    let satisfy (f:char -> bool) (input:CharStream) =
        if input.Length = 0 then Fail 0
        else 
            let result = input.Item 0
            if f result 
            then  Success(result, 1)
            else Fail 0

    let opt (p:Parser<'TResult>) (input:CharStream) =
        match p input with
        | Success (result, next) -> Success(Some result, next)
        | _ -> Success(None, 0)

    // orElse
    let (<|>%) (p:Parser<_>) alt =
        p |> opt |>> (function | Some x -> x | _ -> alt)

    let parse (p:Parser<_>) =
        let p = p .>> eof

        let parse (input:String) = p (CharStream.Create input)  
        parse

    let sepBy1 (delim:Parser<_>) (p:Parser<_>) =
        let additional = (delim .>>. p) |>> (fun (sep, value) -> value) |> many
        (p .>>. additional) |>> (fun (fst, additional) -> Seq.append [fst] additional) 

    let sepBy (delim:Parser<_>) (p:Parser<_>) =
        (sepBy1 delim p) <|>% Seq.empty

    let pchar c  = satisfy (fun i -> i = c)

    let pstring (str:string) (input:CharStream) =
        if input.Length < str.Length
            then Fail input.Length
        else
            let rec doParse i =
                if i = str.Length
                    then Success (str, i)
                else if (str.Chars i) = (input.Item i)
                    then doParse (i + 1)
                else Fail i

            doParse 0
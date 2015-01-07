namespace FunctionalHttp.Parsing

open System;
open System.Collections.Generic

type internal IParseResult<'TResult> =
    | Success of  result : 'TResult * iNext : int * next : CharStream
    | Fail of iFailed : int
    | Eof

type internal Parser<'TResult> = CharStream -> IParseResult<'TResult>

module internal Parser =         
    // map
    let (|>>) (p:Parser<_>) f (input:CharStream) =
        let result = p input
        match result with
        | Fail i -> Fail i
        | Eof -> Eof
        | Success (result, i, next) -> Success (f result, i, next)

    let (.>>.) (p1:Parser<'T1>) (p2:Parser<'T2>) (input:CharStream) = 
        let fstResult = p1 input
        match fstResult with
        | Fail i -> Fail i
        | Eof -> Eof
        | Success (result1, iFirst, next) -> 
            let sndResult = p2 next

            match sndResult with
            | Fail i2 -> Fail (iFirst + i2)
            | Eof -> Eof 
            | Success (result2, i2nd, next) -> Success ((result1, result2), iFirst + i2nd, next)
     
    let (>>.) (p1:Parser<_>) (p2:Parser<_>) = 
        p1 .>>. p2 |>> fun (r1,r2) -> r2

    let (.>>) (p1:Parser<_>) (p2:Parser<_>) = 
        p1 .>>. p2 |>> fun (r1,r2) -> r1

    // NOTE: Doesn't apper to be a corresponding FParsec combinator to this
    let (<^>) (p1:Parser<_>) (p2:Parser<_>) (input:CharStream) = 
        match (p1 input) with 
        | Success (result, i, next) -> 
            Success (Choice1Of2 result, i, next)
        | _ -> 
            match (p2 input) with
            | Success (result, i, next) -> Success (Choice2Of2 result, i, next)
            | Fail i -> Fail i
            | Eof -> Eof

    let (<|>) (p1:Parser<_>) (p2:Parser<_>) = 
        let p = p1 <^> p2
        fun (input:CharStream) ->
            match p input with
            | Fail i -> Fail i
            | Eof -> Eof
            | Success (Choice1Of2 result, i, next) -> Success (result, i, next)
            | Success (Choice2Of2 result, i, next) -> Success (result, i, next)

    let eof (input:CharStream) =
        if input.Length = 0 
        then Success((), 0, input)
        else Fail 0

    let followedBy (pnext:Parser<_>) (input:CharStream) =
        match pnext input with
        | Success _ -> Success ((), 0, input)
        | Eof -> Eof 
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
            | Success (result, i, next) -> 
                index := !index + i
                result::(doParse next)
            | Eof ->
                remainder := input
                []
            | Fail i-> 
                remainder := input
                []
        
        let result = doParse input :> _ seq
        Success (result, !index, !remainder)
     
    let many1 (p:Parser<_>) (input:CharStream) =   
       match (many p input) with
       | Fail i -> Fail i
       | Eof -> Eof
       | Success (result, i, next) as success ->
           if Seq.isEmpty result
           then Fail 0
           else success

    let satisfy (f:char -> bool) (input:CharStream) =
        if input.Length = 0
        then Eof
        else if not (f (input.Item 0))
        then Fail 0
        else 
            let resultValue = input.Item 0
            Success(resultValue, 1, input.SubSequence(1))

    let opt (p:Parser<'TResult>) (input:CharStream) =
        match p input with
        | Success (result, i, next) -> Success(Some result, i, next)
        | _ -> Success(None, 0, input)

    // orElse
    let (<|>%) (p:Parser<_>) alt =
        p |> opt |>> (function | Some x -> x | _ -> alt)
    
    let private parseStream (p:Parser<_>) (input:CharStream) =
        match p input with
        | Success (result, i, next) -> 
            if next.Length = 0 then  Success (result, i, next) else Fail i
        | Fail i -> Fail i
        | Eof -> Eof

    let parse (p:Parser<_>) (input:String) =
        match parseStream p (CharStream input) with
        | Fail i -> None
        | Eof -> None
        | Success (result, _, _) -> Some result  

    let sepBy1 (delim:Parser<_>) (p:Parser<_>) =
        let additional = (delim .>>. p) |>> (fun (sep, value) -> value) |> many
        (p .>>. additional) |>> (fun (fst, additional) -> Seq.append [fst] additional) 

    let sepBy (delim:Parser<_>) (p:Parser<_>) =
        (sepBy1 delim p) <|>% Seq.empty

    let pchar c (input:CharStream) =
        satisfy (fun i -> i = c) input

    let pstring (str:string) (input:CharStream) =
        if input.Length < str.Length
            then Eof
        else
            let rec doParse i =
                if i = str.Length
                    then Success (str, i, input.SubSequence(i))
                else if (str.Chars i) = (input.Item i)
                    then doParse (i + 1)
                else Fail i

            doParse 0
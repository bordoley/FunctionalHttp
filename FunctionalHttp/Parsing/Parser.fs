namespace FunctionalHttp.Parsing

open System;
open System.Collections.Generic

type internal IParseResult<'TResult> =
    | Success of  result : 'TResult * next : CharStream
    | Fail of input : CharStream
    | Eof of input : CharStream

type internal Parser<'TResult> = CharStream -> IParseResult<'TResult>

module internal Parser =         
    // map
    let (|>>) (p:Parser<_>) f (input:CharStream) =
        let result = p input
        match result with
        | Fail _ -> Fail input
        | Eof _ -> Eof input
        | Success (result, next) -> Success (f result, next)

    let (.>>.) (p1:Parser<'T1>) (p2:Parser<'T2>) (input:CharStream) = 
        let fstResult = p1 input
        match fstResult with
        | Fail _ -> Fail input
        | Eof _ -> Eof input
        | Success (result1, next) -> 
            let sndResult = p2 next

            match sndResult with
            | Fail _ -> Fail input
            | Eof _ -> Eof input
            | Success (result2, next) -> Success ((result1, result2), next)
     
    let (>>.) (p1:Parser<_>) (p2:Parser<_>) = 
        p1 .>>. p2 |>> fun (r1,r2) -> r2

    let (.>>) (p1:Parser<_>) (p2:Parser<_>) = 
        p1 .>>. p2 |>> fun (r1,r2) -> r1

    // NOTE: Doesn't apper to be a corresponding FParsec combinator to this
    let (<^>) (p1:Parser<_>) (p2:Parser<_>) (input:CharStream) = 
        match (p1 input) with 
        | Success (result, next) -> Success ((Choice1Of2 result), next)
        | _ -> 
            match (p2 input) with
            | Success (result, next) -> Success ((Choice2Of2 result), next)
            | Fail _ -> Fail input
            | Eof _ -> Eof input

    let (<|>) (p1:Parser<_>) (p2:Parser<_>) = 
        let p = p1 <^> p2
        fun (input:CharStream) ->
            match p input with
            | Fail _ -> Fail input
            | Eof _ -> Eof input
            | Success (Choice1Of2 result, next) -> Success (result, next)
            | Success (Choice2Of2 result, next) -> Success (result, next)

    let eof (input:CharStream) =
        if input.Length = 0 
        then Success((), input)
        else Fail input

    let followedBy (pnext:Parser<_>) (p:Parser<_>) (input:CharStream) =
        match p input with
        | Success (_, next) as result ->
            match pnext next with
            | Success _ -> result
            | Eof _ -> Eof input
            | Fail _ -> Fail input
        | x as result -> result

    let createParserForwardedToRef () =
        let dummy (input:CharStream) = failwith "a parser created with createParserForwardedToRef was not initialized"
        let r = ref dummy
        (fun input -> !r input), r : Parser<_> * Parser<_> ref

    let many (p:Parser<_>) (input:CharStream) =
        let remainder : CharStream ref = ref input

        let rec doParse input =
            let result = p input

            match result with
            | Success (result, next) -> result::(doParse next)
            | Eof next ->
                remainder := next
                []
            | Fail next -> 
                remainder := next
                []
        
        let result = doParse input :> _ seq
        Success (result, !remainder)
     
    let many1 (p:Parser<_>) (input:CharStream) =   
       match (many p input) with
       | Fail _ -> Fail input
       | Eof _ -> Eof input
       | Success (result, next) ->
           if Seq.isEmpty result
           then Fail input
           else Success (result, next)

    let satisfy (f:char -> bool) (input:CharStream) =
        if input.Length = 0
        then Eof input
        else if not (f (input.Item 0))
        then Fail input
        else 
            let resultValue = input.Item 0
            Success(resultValue, input.SubSequence(1))

    let opt (p:Parser<'TResult>) (input:CharStream) =
        match p input with
        | Success (result, next) -> Success(Some result, next)
        | _ -> Success(None, input)

    // orElse
    let (<|>%) (p:Parser<_>) alt =
        p |> opt |>> (function | Some x -> x | _ -> alt)
    
    let private parseStream (p:Parser<_>) (input:CharStream) =
        match p input with
        | Success (result, next) -> 
            if next.Length = 0 then  Success (result, next) else Fail input
        | _ -> Fail input

    let parse (p:Parser<_>) (input:String) =
        match parseStream p (CharStream input) with
        | Fail _ -> None
        | Eof _ -> None
        | Success (result,_) -> Some result  

    let sepBy1 (delim:Parser<_>) (p:Parser<_>) =
        let additional = (delim .>>. p) |>> (fun (sep, value) -> value) |> many
        (p .>>. additional) |>> (fun (fst, additional) -> Seq.append [fst] additional) 

    let sepBy (delim:Parser<_>) (p:Parser<_>) =
        (sepBy1 delim p) <|>% Seq.empty

    let pchar c (input:CharStream) =
        satisfy (fun i -> i = c) input

    let pstring (str:string) (input:CharStream) =
        if input.Length < str.Length
            then Eof input
        else
            let rec doParse i =
                if i = str.Length
                    then Success (str, input.SubSequence(i))
                else if (str.Chars i) = (input.Item i)
                    then doParse (i + 1)
                else Fail input

            doParse 0
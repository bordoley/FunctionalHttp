namespace FunctionalHttp

open System

module internal Parser =
    let (<+>) (p1:Parser<'TToken, 'T1>) (p2:Parser<'TToken, 'T2>) (input:IInput<'TToken>) = 
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
     
    let (<^>) (p1:Parser<'TToken, 'T1>) (p2:Parser<'TToken, 'T2>) (input:IInput<'TToken>) = 
        match (p1 input) with 
        | Success (result, next) -> Success ((Choice1Of2 result), next)
        | _ -> 
            match (p2 input) with
            | Success (result, next) -> Success ((Choice2Of2 result), next)
            | Fail _ -> Fail input
            | Eof _ -> Eof input

    let (<|>) (p1:Parser<'TToken, 'T>) (p2:Parser<'TToken, 'T>) = 
        let p = p1 <^> p2
        fun (input:IInput<'TToken>) ->
            match p input with
            | Fail _ -> Fail input
            | Eof _ -> Eof input
            | Success (Choice1Of2 result, next) -> Success (result, next)
            | Success (Choice2Of2 result, next) -> Success (result, next)

    let eof (input:IInput<'TToken>) =
        if input.Length = 0 
        then Success((), input)
        else Fail input

    let followedBy (pnext:Parser<'TToken, _>) (p:Parser<'TToken,'TResult>) (input:IInput<'TToken>) =
        match p input with
        | Success (_, next) as result ->
            match pnext next with
            | Success _ -> result
            | Eof _ -> Eof input
            | Fail _ -> Fail input
        | x as result -> result

    let many (p:Parser<'TToken,'TResult>) (input:IInput<'TToken>) =
        let remainder : IInput<'TToken> ref = ref input

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
        
        let result = doParse input :> 'TResult seq
        Success (result, !remainder)
     
    let many1 (p:Parser<'TToken,'TResult>) (input:IInput<'TToken>) =   
       match (many p input) with
       | Fail _ -> Fail input
       | Eof _ -> Eof input
       | Success (result, next) ->
           if Seq.isEmpty result
           then Fail input
           else Success (result, next)

    let matches (f:'TToken->bool) (input:IInput<'TToken>) =
        if input.Length = 0
        then Eof input
        else if not (f (input.Item 0))
        then Fail input
        else 
            let resultValue = input.Item 0
            Success(resultValue, input.SubSequence(1))
        
    let map f (p:Parser<'TToken,'TResult>) (input:IInput<'TToken>) =
        let result = p input
        match result with
        | Fail _ -> Fail input
        | Eof _ -> Eof input
        | Success (result, next) -> Success (f result, next)

    let optional (p:Parser<'TToken,'TResult>) (input:IInput<'TToken>) =
        match p input with
        | Success (result, next) -> Success(Some result, next)
        | _ -> Success(None, input)

    let orElse (p:Parser<'TToken,'TResult>) (alt:'TResult) =
        p |> optional |> map (fun opt -> match opt with| Some x -> x | _ -> alt)
    
    let parse (p:Parser<'TToken,'TResult>) (input:IInput<'TToken>) =
        match p input with
        | Success (result, next) -> 
            if next.Length = 0 then  Success (result, next) else Fail input
        | _ -> Fail input

    let sepBy1 (delim:Parser<'TToken, _>) (p:Parser<'TToken,'TResult>) =
        let additional = (delim <+> p) |> map(fun (sep, value) -> value) |> many
        p <+> additional |> map (fun (fst, additional) -> Seq.append [fst] additional) 

    let sepBy (delim:Parser<'TToken, _>) (p:Parser<'TToken,'TResult>) =
        (sepBy1 delim p |> orElse) Seq.empty

    let token t (input:IInput<'TToken>) =
        matches (fun i -> i = t) input

namespace FunctionalHttp.Parsing

open System
open System.Text.RegularExpressions

type internal CharMatcher = char -> bool

[<AutoOpen>]
module internal CharMatchers =
    let isAnyOf (chars:string) =
        // FIXME: Probably add some heuristics here based upon the number of chars in the string
        // and switch the implementation
        let chars = chars.ToCharArray()
        Array.sortInPlace chars
        fun c -> Array.BinarySearch(chars, c) > 0

    let inRange (start:char) (last:char) (c:char) = 
        c >= start && c <= last

    let isDigit = inRange '0' '9'

[<AutoOpen>]
module internal CharParsers =
    let manySatisfy (matcher:CharMatcher) =
        let rec findLast index (input:CharStream) =
            if index = input.Length then index
            else if matcher (input.Item index)
                then findLast (index + 1) input
            else index
            
        let parse (input:CharStream) =
            let result = findLast 0 input

            Success (input.ToString(0, result), result)

        parse

    let many1Satisfy (matcher:CharMatcher) =
        let p = manySatisfy matcher

        let doParse input = 
            match p input with
            | Success (value, next) when value.Length = 0 -> Fail 0
            | result -> result

        doParse
            
    let satisfy (f:CharMatcher) (input:CharStream) =
        if input.Length = 0 then Fail 0
        else 
            let result = input.Item 0
            if f result 
            then  Success(result, 1)
            else Fail 0

    let pchar c  = satisfy (fun i -> i = c)

    let manyMinMaxSatisfy minCount maxCount  (f:CharMatcher) =
        let rec findLast index (input:CharStream) =
            if index = input.Length then index
            else if index = (maxCount + 1) then index
            else if f input.[index]
                then findLast (index + 1) input
            else index

        let parse (input:CharStream) =
            let index = findLast 0 input
            if index >= minCount then Success (input.ToString(0, index), index)
            else Fail (index - 1)

        parse

    let regex pattern =
        let pattern = "\G" + pattern
        let regex = Regex(pattern, RegexOptions.Multiline ||| RegexOptions.ExplicitCapture)

        let parse (input:CharStream) =
            let result = regex.Match(input.str, input.offset)
            if not result.Success then Fail 0
            else Success (result.Value, result.Length)

        parse



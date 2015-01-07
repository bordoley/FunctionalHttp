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

module internal CharParsers =
    let manySatisfy (matcher:CharMatcher) (input: CharStream) =
        let rec findLast index =
            if index = input.Length
            then index
            else if matcher (input.Item index)
            then findLast (index + 1)
            else index

        let result = findLast 0

        Success (input.ToString(0, result), result)

    let many1Satisfy (matcher:CharMatcher) (input: CharStream) =
        let result = manySatisfy matcher input
        match result with
        | Success (value, next) -> 
            if value.Length = 0 then Fail 0 else result
        | _ -> result
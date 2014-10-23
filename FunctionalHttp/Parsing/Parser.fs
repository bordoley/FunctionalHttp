namespace FunctionalHttp

open System;
open System.Collections.Generic

type internal IInput<'T> = 
    abstract Length : int
    abstract Item: int -> 'T
    abstract SubSequence: int -> IInput<'T>
    abstract SubSequence: int*int -> IInput<'T>

type internal IParseResult<'TToken, 'TResult> =
    | Success of  result : 'TResult * next : IInput<'TToken>
    | Fail of input : IInput<'TToken>
    | Eof of input : IInput<'TToken>

type internal Parser<'TToken,'TResult> = IInput<'TToken> -> IParseResult<'TToken, 'TResult>

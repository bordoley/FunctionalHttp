﻿namespace FunctionalHttp.Core
open FunctionalHttp.Parsing

open CharParsers
open HttpParsers
open Abnf

type Challenge = 
    private {
        scheme:string
        dataOrParameters: Choice<string, Map<string,string>>
    }

    member this.DataOrParameters with get() = this.dataOrParameters
    member this.Scheme with get() = this.scheme

    override this.ToString() =
        this.scheme + " " +
        match this.dataOrParameters with
        | Choice1Of2 token68 -> token68
        | Choice2Of2 parameters -> 
            Map.toSeq parameters
            |> Seq.map (function
                | (key, value) when value.Length = 0 -> key
                | (key, value) when key = "realm" ->
                    key + "=" + HttpEncoding.asQuotedString value
                | (key, value) ->
                    key + "=" + HttpEncoding.asTokenOrQuotedString value)
            |> String.concat ", "

    static member internal Parser =
        let auth_scheme = token
        let auth_param = 
            token .>> (BWS .>>. pEquals .>>. BWS) .>>. (token <|> quoted_string)
            |>> fun (k, v) -> 
                (k.ToLowerInvariant(), v)

        let auth_params =
             sepBy1 (opt auth_param)  OWS_COMMA_OWS |>> (fun pairs -> 
                pairs |> Seq.filter Option.isSome |> Seq.map Option.get)
             |>> Map.ofSeq

        let data = token68 .>> followedBy (eof <^> (OWS .>>. pComma))

        auth_scheme .>> (many1Satisfy SP) .>>. ( data <^> auth_params )
        |>> fun (scheme, dataOrParameters) -> 
            { scheme = scheme; dataOrParameters = dataOrParameters; }
        
    static member OAuthToken token = 
        match token |> parse token68 with
        | Success (x, _)-> {scheme = "OAuth"; dataOrParameters = Choice1Of2 token }
        | _ -> invalidArg "token" "Token must be valid base64 data"
      
type Credentials = Challenge

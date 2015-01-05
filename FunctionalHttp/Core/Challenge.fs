﻿namespace FunctionalHttp.Core
open FunctionalHttp.Parsing

open FunctionalHttp.Core.HttpParsers
open FunctionalHttp.Parsing.Parser

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
            |> System.String.Concat      

    static member internal Parser =
        let auth_scheme = token
        let auth_param = 
            token .>>. BWS .>>. (satisfy CharMatchers.EQUALS) .>>. BWS .>>. (token <|> quoted_string) 
            |>> (fun ((((key, _), _ ), _), value) -> (key.ToLowerInvariant(), value))
        
        let auth_params =
             (opt auth_param) |> sepBy1 OWS_COMMA_OWS |>> (fun pairs -> 
                pairs |> Seq.filter Option.isSome |> Seq.map Option.get)
             |>> Map.ofSeq

        let data = token68 |> followedBy (Parser.eof <^> (OWS .>>. pchar ','))

        auth_scheme .>>. (CharMatchers.many1 CharMatchers.SP) .>>. ( data  <^> auth_params )
        |>> (fun ((scheme, _), dataOrParameters) -> 
            { scheme = scheme; dataOrParameters = dataOrParameters; })
        
    static member OAuthToken token = 
        // FIXME: Validate the token is base64 data
        {scheme = "OAuth"; dataOrParameters = Choice1Of2 token }
      
type Credentials = Challenge

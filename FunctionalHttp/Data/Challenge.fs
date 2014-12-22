namespace FunctionalHttp
open FunctionalHttp.Parsing

open FunctionalHttp.HttpParsers
open FunctionalHttp.Parsing.Parser

type Challenge = 
    private {
        scheme:string
        dataOrParameters: Choice<string, Map<string,string>>
    }

    static member internal Parser =
        let auth_scheme = token
        let auth_param = 
            token <+> BWS <+> (matches CharMatchers.EQUALS) <+> BWS <+> (token <|> quoted_string) 
            |> Parser.map (fun ((((key, _), _ ), _), value) -> (key.ToLowerInvariant(), value))
        
        let auth_params =
             (optional auth_param) 
             |> sepBy1 OWS_COMMA_OWS
             |> map (fun pairs -> 
                pairs 
                |> Seq.filter (fun pair -> Option.isSome pair)
                |> Seq.map (fun pair -> pair.Value))
             |> Parser.map (fun pairs -> Map.ofSeq pairs)

        let data = token68 |> followedBy (Parser.eof <^> (OWS <+> parseChar ','))

        auth_scheme <+> (CharMatchers.many1 CharMatchers.SP) <+> ( data  <^> auth_params )
        |> Parser.map (fun ((scheme, _), dataOrParameters) -> 
            { scheme = scheme; dataOrParameters = dataOrParameters; })
        
    static member OAuthToken token = 
        // FIXME: Validate the token is base64 data
        {scheme = "OAuth"; dataOrParameters = Choice1Of2 token }

    member this.DataOrParameters with get() = this.dataOrParameters
    member this.Scheme with get() = this.scheme

    override this.ToString() =
        this.scheme + " " +
        match this.dataOrParameters with
        | Choice1Of2 token68 -> token68
        | Choice2Of2 parameters -> 
            Map.toSeq parameters
            |> Seq.map (fun (key,value) -> 
                match (key, value) with
                | (key, value) when value.Length = 0 -> key
                | (key, value) when key = "realm" ->
                    key + "=" + HttpEncoding.asQuotedString value
                | (key, value) ->
                    key + "=" + HttpEncoding.asTokenOrQuotedString value)
            |> System.String.Concat            

type Credentials = Challenge

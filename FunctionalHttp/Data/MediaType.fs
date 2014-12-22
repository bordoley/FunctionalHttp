namespace FunctionalHttp

open FunctionalHttp.Parsing
open System
open System.Collections.Generic
open System.Linq

open FunctionalHttp.Parsing.CharMatchers
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.HttpParsers

type MediaType = 
    private {
         _type:string
         subType:string
         charset:Option<Charset>
         parameters:Map<string, string>
    }

    static member internal Parser = 
        let parameter = token <+> (parseChar '=') <+> (token <|> quoted_string) |> map (fun x ->
            // The type, subtype, and parameter name tokens are case-insensitive.
            match x with | (key, _), value -> (key.ToLowerInvariant(), value))

        let parameters = OWS_SEMICOLON_OWS <+> parameter |> map (fun x -> 
            match x with | (_, pair) -> pair) |> many

        token <+> (parseChar '/') <+> token <+> parameters |>  map (fun x ->
            match x with 
            | (((_type, _), subType), parameters) ->  
                let charset = ref None

                let parameters =
                    parameters  
                    |> Seq.filter (fun (k,v) ->
                        match (k, !charset) with
                        | ("charset", None) ->
                            match parse Charset.Parser v with
                            | Some result ->
                                charset := Some result
                                false
                            | _ -> false
                        | ("charset", _) -> false
                        | _ -> true)
                    |> Map.ofSeq

                // The type, subtype, and parameter name tokens are case-insensitive.
                { _type = _type.ToLowerInvariant(); subType = subType.ToLowerInvariant(); charset = None; parameters = parameters}
        )

    member this.Type with get() = this._type

    member this.SubType with get() = this.subType

    member this.Charset with get() = this.charset

    member this.Parameters with get() = this.parameters

    // FIXME: Parameters
    override this.ToString() =
            this.Type + "/" + 
            this.SubType + 
            if Option.isSome this.Charset then "; charset=" + this.Charset.Value.ToString() else "" //+
            

[<AutoOpen>]
module MediaTypeExtension =
    type MediaType with
        member this.With(?_type:String, ?subType:String, ?charset:Charset, ?parameters:'K*'V seq) =
            match (_type, subType, charset, parameters) with
            | (None,None, _ , None) -> 
                {
                    _type = this.Type
                    subType = this.SubType;
                    charset = if Option.isSome charset then charset else this.Charset
                    parameters = this.Parameters;
                }

        member this.Without(?charset, ?parameters) = {
                _type = this.Type; 
                subType = this.SubType;
                charset = if Option.isSome charset then None else this.Charset;
                parameters = if Option.isSome parameters then Map.empty else this.Parameters;
            }

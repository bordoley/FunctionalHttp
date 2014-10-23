namespace FunctionalHttp

open System
open System.Collections.Generic
open System.Linq

open FunctionalHttp.CharMatchers
open FunctionalHttp.Parser
open FunctionalHttp.HttpParsers

type MediaType = 
    private {
         _type:string
         subType:string
         charset:Option<Charset>
         parameters:Map<string, string>
    }

    static member private parameter = token <+> (matches EQUALS) <+> (token <|> quoted_string) |> map (fun x ->
        match x with | (key, _), value -> (key, value))

    static member private paramps = OWS_SEMICOLON_OWS <+> MediaType.parameter |> map (fun x -> 
        match x with | (_, pair) -> pair) |> many

    static member internal Parser = 
        token <+> (matches FORWARD_SLASH) <+> token <+> MediaType.paramps |>  map (fun x ->
            match x with | (((_type, _), subType), parameters) ->  
                { _type = _type; subType = subType; charset = None; parameters = Map.empty}
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
            //parameters.Select(fun x ->


type MediaRange =
    private {
         _type:string
         subType:string
         charset:Option<Charset>
         parameters:Map<string, string>
    }

    member this.Type with get() = this._type

    member this.SubType with get() = this.subType

    member this.Charset with get() = this.charset

    member this.Parameters with get() = this.parameters :> KeyValuePair<string,string> seq

    // FIXME: Parameters
    override this.ToString() =
            this.Type + "/" + 
            this.SubType + 
            if Option.isSome this.Charset then "; charset=" + this.Charset.Value.ToString() else "" //+
            //parameters.Select(fun x ->

﻿namespace FunctionalHttp.Core

open FunctionalHttp.Collections
open Sparse
open System
open System.Collections.Generic
open System.Linq
open System.Runtime.CompilerServices

open HttpParsers

type MediaType = 
    private {
         _type:string
         subType:string
         charset:Option<Charset>

         // FIXME: Should be a multimap
         parameters:Map<string, string>
    }

    member this.Type with get() = this._type

    member this.SubType with get() = this.subType

    member this.Charset with get() = this.charset

    member this.Parameters with get() = this.parameters

    // FIXME: Parameters
    override this.ToString() =
            this.Type + "/" + 
            this.SubType + 
            (if Option.isSome this.Charset then "; charset=" + this.Charset.Value.ToString() else "") +
            (this.Parameters |> Map.toSeq |> Seq.map (fun (k,v) -> k + "=" + (HttpEncoding.asTokenOrQuotedString v)) |> String.concat ";")

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module MediaType =
    let internal parser = 
        let keyNotQ = (token >>= function | key when key <> "q" -> preturn key | _ -> pzero)

        let parameter = 
            token
            .>> pEquals .>>. (token <|> quoted_string) 
            // The type, subtype, and parameter name tokens are case-insensitive.
            |>> function (key, value) -> (key.ToLowerInvariant(), value)
 
        let parameters = (OWS_SEMICOLON_OWS >>. parameter) |> many

        token .>> pForwardSlash .>>. token .>>. parameters |>> (fun ((_type, subType), parameters) ->  
                let charset = ref None

                let parameters =
                    parameters  
                    |> Seq.filter (fun (k,v) ->
                        match (k, !charset) with
                        | ("charset", None) ->
                            match parse Charset.parser v with
                            | Success (result, _) ->
                                charset := Some result
                                false
                            | _ -> false
                        | ("charset", _) -> false
                        | _ -> true)
                    |> Map.ofSeq

                // The type, subtype, and parameter name tokens are case-insensitive.
                { _type = _type.ToLowerInvariant(); subType = subType.ToLowerInvariant(); charset = !charset; parameters = parameters}
        )

    let create (input:string) =
        match (parse parser input) with
        | Success (result, next) when next = input.Length -> result
        | _ -> failwith "Not a valid media-type"

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

        [<Extension>]
        member this.TryGetCharset(charset : byref<Charset>) = 
            Option.tryGetValue this.Charset &charset
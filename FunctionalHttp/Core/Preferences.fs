namespace FunctionalHttp.Core

open Sparse
open System

open Abnf
open HttpParsers
open Predicates

type Preference<'T> =
    private {
        value:Choice<'T, Any>
        quality:uint16
    }

    member this.Value with get() = this.value

    member this.Quality with get() = this.quality

    override this.ToString() =
        (match this.Value with
        | Choice1Of2 v -> v.ToString()
        | _ -> Any.instance.ToString()) + 

        // FIXME: Make this print pretty
        if this.Quality < 1000us then ";q=0." + string this.Quality else ""

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Preference = 
    let parser (p:Parser<'T>) =
        let qvalue = 
            let qvalue0 = 
                (pchar '0' >>. opt (pPeriod >>. manyMinMaxSatisfy 0 3 isDigit) ) 
                |>> (function 
                    | None -> 0us 
                    // FIXME: UInt16.Parse can throw
                    | Some x when x.Length = 1 -> (UInt16.Parse x) * 100us
                    | Some x when x.Length = 2 -> (UInt16.Parse x) * 10us
                    | Some x  -> UInt16.Parse x)

            let qvalue1 = (pchar '1' >>. opt (pPeriod >>. manyMinMaxSatisfy 0 3 (fun c -> c = '0'))) |>> (fun _ -> 1000us)

            qvalue0 <|> qvalue1

        let weight = opt ((OWS_SEMICOLON_OWS >>. pstring "q=") >>. qvalue) |>> (function | None -> 1000us | Some x -> x)

        (Any.parser <^> p) .>>. weight
        |>> fun (value, quality) -> 
            match value with
            | Choice1Of2 any -> { value = Choice2Of2 Any.instance; quality = quality } 
            | Choice2Of2 v -> { value = Choice1Of2 v; quality = quality } 

type AcceptPreference =
    private {
        mediaRange:MediaRange
        quality:uint16
        parameters:Map<string,string>
    }

    member this.MediaRange with get () = this.mediaRange

    member this.Quality with get () = this.quality

    member this.Parameters with get () = this.parameters

    override this.ToString() =
        let parameters = (this.parameters |> Map.toSeq |> Seq.map (fun (k,v) -> k + "=" + (HttpEncoding.asTokenOrQuotedString v)) |> String.concat ";")

        (string this.mediaRange) + 
        (if this.quality < 1000us then ";q=0." + string this.quality else "") + 
        ";" + parameters

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal AcceptPreference = 
    let parser =
        let qvalue = 
            let qvalue0 = 
                (pchar '0' >>. opt (pPeriod >>. manyMinMaxSatisfy 0 3 isDigit) ) 
                |>> (function 
                    | None -> 0us 
                    | Some x when x.Length = 1 -> (UInt16.Parse x) * 100us
                    | Some x when x.Length = 2 -> (UInt16.Parse x) * 10us
                    | Some x  -> UInt16.Parse x)

            let qvalue1 = (pchar '1' >>. opt (pPeriod >>. manyMinMaxSatisfy 0 3 (fun c -> c = '0'))) |>> (fun _ -> 1000us)

            qvalue0 <|> qvalue1

        let weight = opt ((OWS_SEMICOLON_OWS >>. pstring "q=") >>. qvalue) |>> (function | None -> 1000us | Some x -> x)

        let parameter = 
            token .>>. ((pEquals >>. (token <|> quoted_string)) <|>% "")
            // The type, subtype, and parameter name tokens are case-insensitive.
            |>> fun (key, value) -> (key.ToLowerInvariant(), value)
 
        let parameters = (OWS_SEMICOLON_OWS >>. parameter) |> many

        MediaRange.parser .>>. weight .>>. parameters
        |>> fun ((mr, quality), parameters) ->
            { mediaRange = mr; quality = quality; parameters = parameters |> Map.ofSeq }
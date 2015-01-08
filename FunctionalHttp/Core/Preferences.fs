namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open FunctionalHttp.Parsing.CharMatchers
open FunctionalHttp.Parsing.CharParsers
open FunctionalHttp.Parsing.Parser
open FunctionalHttp.Core.Abnf
open FunctionalHttp.Core.HttpParsers

open System

type Preference<'T> =
    private {
        value:'T
        quality:uint16
    }

    member this.Value with get() = this.value
    member this.Quality with get() = this.quality

    override this.ToString() =
        (this.value.ToString()) + 
        // FIXME: Make this print pretty
        if this.Quality < 1000us then ";q=0." + string this.Quality else ""

    static member internal Parser (p:Parser<'T>) =
        let qvalue = 
            let qvalue0 = 
                (pchar '0' >>. opt (pchar '.' >>. times 0 3 DIGIT) ) 
                |>> (function 
                    | None -> 0us 
                    | Some x when x.Length = 1 -> (UInt16.Parse x) * 100us
                    | Some x when x.Length = 2 -> (UInt16.Parse x) * 10us
                    | Some x  -> UInt16.Parse x)

            let qvalue1 = (pchar '1' >>. opt (pchar '.' >>. times 0 3 (fun c -> c = '0'))) |>> (fun _ -> 1000us)

            qvalue0 <|> qvalue1

        let weight = opt ((OWS_SEMICOLON_OWS >>. pstring "q=") >>. qvalue) |>> (function | None -> 1000us | Some x -> x)

        p .>>. weight
        |>> fun (value, quality) -> { value = value; quality = quality } 


type PreferenceWithParams<'T> =
    private {
        preference:Preference<'T>
        parameters:List<string*string>
    }

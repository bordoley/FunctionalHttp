namespace FunctionalHttp.Core

open FunctionalHttp.Parsing
open System

open Abnf
open CharParsers
open HttpParsers

type Warning = 
    private {
        code:uint16
        agent:Choice<HostPort,String>
        text:string
        date:DateTime option
    }

    override this.ToString() =
        (this.code.ToString("000")) + " " + 
        (match this.agent with | Choice1Of2 hostPort -> string hostPort | Choice2Of2 pseudonym -> pseudonym) + 
        " " + (HttpEncoding.asQuotedString this.text) + 
        match this.date with| None -> "" | Some date -> "\"" + HttpEncoding.dateToHttpDate date + "\""

    static member internal Parser =
        // Fixme : UInt16.Parse can throw
        let code = manyMinMaxSatisfy 3 3 isDigit |>> UInt16.Parse
        let agent = HostPort.Parser <^> token

        code .>> pSpace .>>. agent .>> pSpace .>>. quoted_string .>>. opt httpDate
        |>> fun (((code, agent), text), date) -> 
            { code = code; agent = agent; text = text; date = date }
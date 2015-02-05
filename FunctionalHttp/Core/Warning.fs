namespace FunctionalHttp.Core

open Sparse
open System

open Abnf
open HttpParsers
open Predicates

type Warning = 
    private {
        code:uint16
        agent:Choice<HostPort,String>
        text:string
        date:DateTime option
    }

    member this.Code with get() = this.code

    member this.Agent with get() = this.agent

    member this.Text with get() = this.text

    member this.Date with get() = this.date

    override this.ToString() =
        (this.code.ToString("000")) + " " + 
        (match this.agent with | Choice1Of2 hostPort -> string hostPort | Choice2Of2 pseudonym -> pseudonym) + 
        " " + (HttpEncoding.asQuotedString this.text) + 
        match this.date with| None -> "" | Some date -> "\"" + HttpEncoding.dateToHttpDate date + "\""

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Warning = 
    let parser = 
        // Fixme : UInt16.Parse can throw
        let code = manyMinMaxSatisfy 3 3 isDigit |>> UInt16.Parse
        let agent = HostPort.parser <^> token

        code .>> pSpace .>>. agent .>> pSpace .>>. quoted_string .>>. opt httpDate
        |>> fun (((code, agent), text), date) -> 
            { code = code; agent = agent; text = text; date = date }
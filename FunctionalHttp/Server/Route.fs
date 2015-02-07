namespace FunctionalHttp.Server
open Sparse
open System
open FunctionalHttp.Core

type internal RouteSegment = 
    | Glob
    | GlobParameter of String
    | Parameter of String
    | Segment of String
    | EmptySegment

    override this.ToString() =
        match this with
        | Glob -> "*"
        | GlobParameter v -> "*" + v
        | Parameter v -> ":" + v
        | Segment v -> v
        | EmptySegment -> ""

type Route = 
    private {
        segments:RouteSegment list
    }

    member internal this.ToList() = this.segments

    override this.ToString() = String.Join("/", this.segments)

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Route =
    let parser = 
        let pEmpty = Primitives.preturn ""

        let pNonEmptySegment =
            let pGlob = 
                pAsterisk >>. regex (UriGrammar.pchar + "*") 
                |>> (fun v -> if  v.Length = 0 then Glob else GlobParameter v)

            let pParameter =
                pColon >>. many1Satisfy (fun c -> c <> '/')
                |>> (fun v -> Parameter v)

            let pSegment =
                regex (UriGrammar.pchar + "+") 
                |>> (fun v -> Segment v)
            
            pGlob <|> pParameter <|> pSegment

        pEmpty >>. pForwardSlash >>. sepBy1 pNonEmptySegment pForwardSlash .>> eof >>= (fun result ->
            let segments = Seq.append [EmptySegment] result |> List.ofSeq

            preturn segments)
       |>> (fun segments -> { segments = segments })

    let create route =
        match parse parser route with
        | Success (result, _) -> result

        | _ -> failwith "Not a route"

type Route with
    static member Create route = Route.create route
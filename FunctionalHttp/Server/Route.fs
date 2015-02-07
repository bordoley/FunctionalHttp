namespace FunctionalHttp.Server
open Sparse
open System
open FunctionalHttp.Core

type internal RouteSegment = 
    | Glob of String
    | Parameter of String
    | Segment of String
    | EmptySegment

    override this.ToString() =
        match this with
        | Glob v -> "*" + v
        | Parameter v -> ":" + v
        | Segment v -> v
        | EmptySegment -> ""

type Route = 
    private {
        segments:RouteSegment list
    }

    member internal this.ToList() = this.segments

    override this.ToString() = String.Join("/", this.segments)

[<AutoOpen>]
module internal RouteExtensions =
    type private RouteSegment with
        member this.RouterKey =
            match this with
            | Glob v -> "*"
            | Parameter v -> ":"
            | Segment v -> v
            | EmptySegment -> ""

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Route =
    let parser = 
        let pEmpty = Primitives.preturn ""

        let pNonEmptySegment =
            let pGlob = 
                pAsterisk >>. many1Satisfy (fun c -> c <> '/')
                |>> (fun v -> Glob v)

            let pParameter =
                pColon >>. many1Satisfy (fun c -> c <> '/')
                |>> (fun v -> Parameter v)

            let pSegment =
                regex (UriGrammar.pchar + "+") 
                |>> (fun v -> Segment v)
            
            pGlob <|> pParameter <|> pSegment

        pEmpty >>. pForwardSlash >>. sepBy1 pNonEmptySegment pForwardSlash .>> eof
        |>> (fun segments -> { segments = Seq.append [EmptySegment] segments |> List.ofSeq })

    let rec validate (segments: RouteSegment list) (keys: Set<String>) = 
        match segments with
        | [] -> ()
        | head::tail ->
            let keys =
                match head with
                | Glob v ->
                    if keys.Contains "*" 
                        then failwith "Route contains multiple adjacent glob segments."

                    if keys.Contains v
                        then failwith "Route contains duplicate keys"

                    keys |> Set.add "*" |> Set.add v
                | Parameter v  -> 
                    if keys.Contains "*" 
                        then failwith "Glob segment cannot be terminated by a parameter segment."

                    if keys.Contains v
                        then failwith "Route contains duplicate keys"

                    keys.Add v
                | Segment _ | EmptySegment -> 
                    keys.Remove "*"

            validate tail keys

    let create route =
        match parse parser route with
        | Success (result, _) -> 
            let segments = result.ToList()
            validate segments Set.empty
            result
        | _ -> failwith "Not a route"

type Route with
    static member Create route = Route.create route
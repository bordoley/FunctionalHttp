namespace FunctionalHttp.Server
open Sparse
open System
open FunctionalHttp.Core
open System.Runtime.CompilerServices

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
module internal RouteSegmentExtensions =
    type RouteSegment with
        member this.RouterKey =
            match this with
            | Glob v -> "*"
            | Parameter v -> ":"
            | Segment v -> v
            | EmptySegment -> ""

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Route =
    let private parser = 
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

    let rec private validate (segments: RouteSegment list) (keys: Set<String>) = 
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

    let internal create route =
        match parse parser route with
        | Success (result, _) -> 
            let segments = result.ToList()
            validate segments Set.empty
            result
        | _ -> failwith "Not a route"

    let private getParametersFromPath (route:Route) (path:string list) =
        let rec getParams (segments: RouteSegment list) (path:string list) (acc:Map<string, string>) =
            match (segments, path) with
            | ([],[]) -> acc
            | ([], _::_) | (_::_, []) -> failwith "path doesn't match the route"
            | (sHead::sTail, pHead::pTail) -> 
                match sHead with
                | Glob v ->
                    let stopSegment =
                        match sTail with
                        | [] -> None
                        | (Segment next)::_ -> Some next
                        | _ -> failwith "Invalid route"

                    let rec matchGlobWithStopSegment (path:List<String>) (acc:List<string>) =
                        match path with
                        | head::tail when (Some head) = stopSegment -> (path, String.concat "/" acc)
                        | _::[] | [] -> failwith "path doesn't match the route"
                        | head::tail ->
                            let acc = head::acc
                            matchGlobWithStopSegment tail acc

                    match stopSegment with
                    | None -> 
                        acc.Add (v, String.concat "/" path)
                    | Some seg ->
                        let (pTail, globMatch) = matchGlobWithStopSegment path []
                        let acc = acc.Add (v, globMatch)
                        getParams sTail pTail acc

                | Parameter v ->
                    let acc = acc |> Map.add v pHead
                    getParams sTail pTail acc
                | Segment v when pHead = v -> 
                    getParams sTail pTail acc
                | EmptySegment when pHead = "" ->
                    getParams sTail pTail acc
                | _ -> failwith "path doesn't match the route"

        getParams (route.ToList()) path Map.empty

    [<Extension;CompiledName("GetParameters")>]
    let getParametersFromUri (route:Route) (uri:Uri) =
        // FIXME: Add uri extensions Uri.ToPath()
        let path = uri.AbsolutePath.Split ([|'/'|], StringSplitOptions.None) |> Seq.toList
        getParametersFromPath route path

    let empty = { segments =[] }

type Route with
    static member Empty = Route.empty

    static member Create route = Route.create route
namespace FunctionalHttp.Server
open FunctionalHttp.Collections

type internal Router = 
    private {
        resource: IServerResource option
        children: Map<string,Router>
    }
    static member Empty = { resource = Option.None; children = Map.empty }

    static member private routeSegmentToKey (segment:string) =
        if segment.StartsWith(":") then ":" else segment

    member this.Item (path: string list) =
        match path with
        | [] -> this.resource
        | head::tail -> 
            let exactMatchResult = 
                this.children.TryFind head 
                |> Option.orElseLazy (lazy (this.children.TryFind ":"))
                |> Option.bind (fun router -> router.Item tail)
            
            let rec globMatch (path: string list) (router: Router) =
                match path with
                | [] | _::[]-> router.resource
                | head::tail ->
                    router.children.TryFind tail.Head
                    |> Option.bind (fun childRouter -> childRouter.Item tail)
                    |> Option.orElseLazy (lazy globMatch tail router) 

            exactMatchResult
            |> Option.orElseLazy (lazy 
                (
                    this.children.TryFind ("*" + head)
                    |> Option.orElseLazy (lazy (this.children.TryFind "*"))
                    |> Option.bind(fun router -> globMatch tail router)
                ))

    member private this.DoAdd (route:string list) (resource:IServerResource) =
        match route with
        | [] -> { resource = Some resource; children = this.children }
        | head::tail -> 
            let key = Router.routeSegmentToKey head
            let newChild = 
                this.children.TryFind key
                |> Option.map (fun router -> router.DoAdd tail resource)
                |> Option.getOrElseLazy (lazy Router.Empty.DoAdd tail resource)
            { resource = this.resource; children = this.children.Add (key, newChild) }

    member this.Add (resource:IServerResource) = this.DoAdd (resource.Route.ToList()) resource

    member this.AddAll (resources:IServerResource seq) =
        resources |> Seq.fold (fun (router:Router) resource -> router.Add resource) this           
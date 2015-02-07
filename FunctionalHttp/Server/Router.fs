namespace FunctionalHttp.Server
open FunctionalHttp.Collections

type internal Router =
    private {
        resource: IStreamResource option
        children: Map<string,Router>
    }

    member this.Item (path: string list) =
        match path with
        | [] -> this.resource
        | head::tail ->                       
            let rec globMatch (path: string list) (router: Router) =
                match path with
                | [] | _::[]-> router.resource
                | head::tail ->
                    router.children.TryFind tail.Head
                    |> Option.bind (fun childRouter -> childRouter.Item tail)
                    |> Option.orElseLazy (lazy globMatch tail router) 

            this.children.TryFind head 
            |> Option.orElseLazy (lazy (this.children.TryFind ":"))
            |> Option.bind (fun router -> router.Item tail)
            |> Option.orElseLazy (lazy 
                (
                    this.children.TryFind ("*")
                    |> Option.bind(fun router -> globMatch tail router)
                ))

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal Router =
    let empty = { resource = Option.None; children = Map.empty }

    let rec private doAdd (route:RouteSegment list) (resource:IStreamResource) (router: Router) =
        match route with
        | [] -> { resource = Some resource; children = router.children }
        | head::tail -> 
            let key = head.RouterKey
            let newChild = 
                router.children.TryFind key
                |> Option.map (fun router -> router |> doAdd tail resource)
                |> Option.getOrElseLazy (lazy (empty |> doAdd tail resource))
            { resource = router.resource; children = router.children.Add (key, newChild) }

    let add (resource:IStreamResource) (router: Router) =
        router |> doAdd (resource.Route.ToList()) resource

    let addAll (resources:IStreamResource seq) (router: Router) =
        resources |> Seq.fold (fun (router:Router) resource -> router |> add resource) router  

type Router with
    member this.Add (resource:IStreamResource) = 
        this |> Router.add resource

    member this.AddAll (resources:IStreamResource seq) = 
        this |> Router.addAll resources     
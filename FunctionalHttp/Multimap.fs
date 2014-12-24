namespace FunctionalHttp.Collections

module internal Multimap =
    let asPairSequence (this:Map<'K,'I> when 'I :> 'V seq) =
        this |> Seq.collect (fun kvp -> kvp.Value |> Seq.map (fun v-> (kvp.Key,v)))

module internal ListMultimap =
    let addPair (k, v) (map:Map<'K,'V list>) =
        let newList = v::(defaultArg (map.TryFind k) List.empty)
        map.Add(k, newList)
    
    let empty = Map.empty<'K, 'V list>

    let fromPairs (pairs:('K*'V) seq) =
        Seq.fold (fun map (k,v) -> map |> addPair (k,v)) empty pairs

    let itemOrEmpty k (this:Map<'K,'V list>) =
        defaultArg (this.TryFind k) List.empty

    let removePair (this:Map<'K,'V list>, k, v) =
        match this.TryFind k with
        | None -> this
        | Some list ->
            let newList = list |> Seq.filter (fun x -> x<>v) |> Seq.toList
            this.Add(k,newList)
   
module internal SetMultimap =
    let addPair (k, v) (map:Map<'K,Set<'V>>) =
        let currentSetValue = defaultArg (map.TryFind k) Set.empty
        match currentSetValue.Contains(v) with
        | true -> map
        | false -> 
            let newSetValue = currentSetValue.Add v
            map.Add(k, newSetValue)
    
    let empty = Map.empty<'K, Set<'V>>

    let fromPairs (pairs:('K*'V) seq) =
        Seq.fold (fun map (k,v) -> map |> addPair (k,v)) empty pairs

    let itemOrEmpty (this:Map<'K,Set<'V>>, k) =
        (defaultArg (this.TryFind k) Set.empty)
    
    let removePair (this:Map<'K,Set<'V>>, k, v) =
        match this.TryFind k with
        | None -> this
        | Some set -> 
            if set.Contains v
            then this.Add(k, set.Remove v)
            else this


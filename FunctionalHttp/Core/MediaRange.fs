namespace FunctionalHttp.Core

open Sparse

open HttpParsers

type MediaRange = 
    private {
        _type:Choice<string,Any>
        subType:Choice<string,Any>

        // FIXME: SHould be a multimap
        parameters:Map<string, string>
    }

    member this.Type with get() = this._type

    member this.SubType with get() = this.subType

    member this.Parameters with get() = this.parameters

    override this.ToString() =
        let _type = match this.Type with | Choice1Of2 t -> t | _ -> Any.Instance.ToString()
        let subType = match this.SubType with | Choice1Of2 t -> t | _ -> Any.Instance.ToString()
        let parameters = this.Parameters |> Map.toSeq |> Seq.map (fun (k,v) -> k + "=" + (HttpEncoding.asTokenOrQuotedString v)) |> String.concat ";"
        _type + "/" + subType + ";" + parameters

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module internal MediaRange = 
    let parser =
        MediaType.parser >>= fun m -> 
            let parameters = 
                match m.Charset with
                | Some c -> m.Parameters |> Map.add "charset" (string c)
                | _ -> m.Parameters

            match (m.Type, m.SubType) with
                | (t, s) when t = "*" && s = "*" -> 
                    let result = {_type = Choice2Of2 Any; subType = Choice2Of2 Any; parameters = parameters }
                    preturn result
                | (t, s) when t <> "*" && s = "*" -> 
                    let result = {_type = Choice1Of2 t; subType = Choice2Of2 Any; parameters = parameters }
                    preturn result
                | (t, s) when t = "*"  &&  s <> "*" -> 
                    pzero
                | (t, s) -> 
                    let result = {_type = Choice1Of2 t; subType = Choice1Of2 s; parameters = parameters }
                    preturn result
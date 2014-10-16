namespace FunctionalHttp

open System
open System.Collections.Generic
open System.Linq

type MediaRange =
    private {
         _type:string
         subType:string
         charset:Option<Charset>
         parameters:Map<string, string seq>
    }

    member this.Type with get() = this._type

    member this.SubType with get() = this.subType

    member this.Charset with get() = this.charset

    // FIXME IEnumerable<KeyValuePair>
    member this.Parameters with get() = this.parameters

    // FIXME: Parameters
    override this.ToString() =
            this.Type + "/" + 
            this.SubType + 
            if Option.isSome this.Charset then "; charset=" + this.Charset.Value.ToString() else "" //+
            //parameters.Select(fun x ->

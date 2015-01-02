namespace FunctionalHttp.Server

open FunctionalHttp.Collections
open FunctionalHttp.Core
open FunctionalHttp.Parsing

open System
open System.Text

type IAuthorizer =
  abstract member AuthenticationChallenge : Challenge with get
  abstract member Scheme:string with get
  abstract member Authenticate: HttpRequest<unit> -> Async<bool>

module Authorizer =
    [<CompiledName("Basic")>]
    let basic (realm:string) (f:HttpRequest<unit>*string*string -> Async<bool>) =
        let challengeString = sprintf "basic realm=\"%s\", encoding=\"UTF-8\"" realm
        let challenge = challengeString |> Parser.parse Challenge.Parser |> Option.get

        { new IAuthorizer with
            member this.AuthenticationChallenge with get () = challenge

            member this.Scheme = "Basic"

            member this.Authenticate (req:HttpRequest<unit>) =
                async {
                    let creds = req.Authorization.Value
                    return! 
                        match creds.DataOrParameters with
                        | Choice1Of2 base64Data ->
                            let bytes = Convert.FromBase64String base64Data
                            let decodedString = Encoding.UTF8.GetString(bytes, 0, bytes.Length)
                            let userPwd = decodedString.Split([|':'|], 2, StringSplitOptions.None)
                            if userPwd.Length <> 2
                            then Async.result false
                            else f(req, userPwd.[0], userPwd.[1])
                        | _ -> async{ return false }
                }
        }
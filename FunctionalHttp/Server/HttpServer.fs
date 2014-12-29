namespace FunctionalHttp.Server

open FunctionalHttp.Core
open System.IO

module internal HttpServer =
    let processRequest (applicationProvider:HttpRequest<Stream> -> IHttpApplication) (req:HttpRequest<Stream>) =
        async {
            let app = applicationProvider(req)
            let req2 = app.Filter req
            let resource = app.Route req2
            let req3 = resource.Filter req2
            let! resp = resource.Handle req3
            let! resp2 =
                if resp.Status <> HttpStatus.informationalContinue
                then resp.ToAsyncResponse()
                else async {
                    let! req4 = resource.Parse req3
                    return! 
                        match req4.Entity with
                        | Some e -> resource.Accept req4
                        | None -> HttpStatus.clientErrorBadRequest.ToAsyncResponse()
                }
            let resp3 = resource.Filter resp2
            return! resource.Serialize (req3, resp3)
        }

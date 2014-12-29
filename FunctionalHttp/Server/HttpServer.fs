namespace FunctionalHttp.Server

open FunctionalHttp.Core
open System.IO

module internal HttpServer =
    let processRequest (applicationProvider:HttpRequest<Stream> -> IHttpApplication) (req:HttpRequest<Stream>) =
        let app = applicationProvider(req)
        let req2 = app.Filter req

        let resource = app.Route req2

        async {
            let! req3 = resource.Parse req2
            let req4 = resource.Filter req3
            let! resp = resource.Handle req4
            let! resp2 =
                if resp.Status <> HttpStatus.informationalContinue
                then resp.ToAsyncResponse()
                else async {
                    let! req4 = resource.Parse req
                    return! 
                        match req4.Entity with
                        | Some e -> resource.Accept req4
                        | None -> HttpStatus.clientErrorBadRequest.ToAsyncResponse()

                }
            let resp3 = resource.Filter resp2
            return! resource.Serialize (req3, resp3)
        }

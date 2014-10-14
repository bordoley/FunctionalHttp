namespace FunctionalHttp

type ChallengeMessage = 
    | Base64 of scheme:string*data:string
    | Parameters of scheme:string*parameters:Map<string,string>

    static member OAuthToken token = 
        // FIXME: Validate the token is base64 data
        Base64 ("OAuth", token)

    override this.ToString() =
        match this with 
        | Base64 (scheme, data) -> scheme + " " + data
        | Parameters (scheme, data) -> "fixme"

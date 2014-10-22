namespace FunctionalHttp

type EncodedChallengeMessage =
    private {
        scheme:string
        data: string
    }

type ParametersChallengeMessage =
    private {
        scheme:string
        parameters:Map<string,string>
    }

open HttpParsers

type ChallengeMessage = 
    | Encoded of EncodedChallengeMessage
    | Parameters of ParametersChallengeMessage

    static member OAuthToken token = 
        // FIXME: Validate the token is base64 data
        Encoded {scheme = "OAuth"; data = token }

    override this.ToString() =
        match this with 
        | Encoded challenge -> challenge.scheme + " " + challenge.data
        | Parameters challenge -> "fixme"

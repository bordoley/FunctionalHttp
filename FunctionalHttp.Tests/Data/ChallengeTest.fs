namespace FunctionalHttp.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp

module ChallengeTests =
    [<Test>]
    let ``test parsing with base64 data`` () =
        let tests = [("Basic   dfjskdlfhjshflkjhfdslhd434543sdfsfsgdfdf=====", "dfjskdlfhjshflkjhfdslhd434543sdfsfsgdfdf=====") ]

        for (test, expected) in tests do
            match Parser.parse Challenge.Parser (test.AsInput()) with
            | Success (result, next) -> 
                match result.DataOrParameters with
                | Choice1Of2 data-> data |> should equal expected
                | _ -> failwith ("Parse failed for test: " + test)
            | _ -> failwith ("Parse failed for test: " + test)

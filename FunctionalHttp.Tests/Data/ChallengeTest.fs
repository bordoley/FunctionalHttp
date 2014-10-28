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

    [<Test>]
    let  ``test parting with parameters`` () =
        let tests = [("Basic realm=\"foo\"", [("realm", "foo")]) ;
                     ("Basic realm=foo",     [("realm", "foo")]) ;
                     ("Basic realm = \"\\f\\o\\o\"", [("realm", "foo")])]


        for (test, expected) in tests do
            match Parser.parse Challenge.Parser (test.AsInput()) with
            | Success (result, next) -> 
                match result.DataOrParameters with
                | Choice2Of2 parameters-> parameters |> should equal (expected |> Map.ofSeq)
                | _ -> failwith ("Parse failed for test: " + test)
            | _ -> failwith ("Parse failed for test: " + test)

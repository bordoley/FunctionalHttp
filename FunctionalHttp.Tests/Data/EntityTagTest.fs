namespace FunctionalHttp.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp

module EntityTagTests =
    [<Test>]
    let ``test parsing`` () =
        let tests = [("W/\"abc\"", true, "abc");
                     ("\"abc\"", false, "abc")]

        for (test, expectedIsWeak, expectedTag) in tests do
            match Parser.parse EntityTag.Parser (test.AsInput()) with
            |  Success (result, next) -> 
                result.IsWeak |> should equal expectedIsWeak
                result.Tag |> should equal expectedTag
            |  _ -> failwith ("Parse failed for test: " + test)


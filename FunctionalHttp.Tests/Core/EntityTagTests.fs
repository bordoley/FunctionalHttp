namespace FunctionalHttp.Core.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp.Core
open FunctionalHttp.Parsing

(*
module EntityTagTests =
    [<Test>]
    let ``test parsing`` () =
        let tests = [("W/\"abc\"", true, "abc");
                     ("\"abc\"", false, "abc")]

        for (test, expectedIsWeak, expectedTag) in tests do
            match Parser.parse EntityTag.Parser test with
            |  Some result -> 
                result.IsWeak |> should equal expectedIsWeak
                result.Tag |> should equal expectedTag
            |  _ -> failwith ("Parse failed for test: " + test)
*)
namespace FunctionalHttp.Core.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp.Parsing
open FunctionalHttp.Core

module ChallengeTests =
    [<Test>]
    let ``test parsing with base64 data`` () =
        let tests = [("Basic   dfjskdlfhjshflkjhfdslhd434543sdfsfsgdfdf=====", "dfjskdlfhjshflkjhfdslhd434543sdfsfsgdfdf=====") ]

        for (test, expected) in tests do
            match Parser.parse Challenge.Parser test with
            | Some result -> 
                match result.DataOrParameters with
                | Choice1Of2 data-> data |> should equal expected
                | _ -> failwith ("Parse failed for test: " + test)
            | _ -> failwith ("Parse failed for test: " + test)

    [<Test>]
    let  ``test parting with parameters`` () =
        let tests = [("Basic realm=\"foo\"",                                "Basic",   [("realm", "foo")]);
                     ("Basic realm=foo",                                    "Basic",   [("realm", "foo")]);
                     ("Basic , realm=\"foo\"",                              "Basic",   [("realm", "foo")]);
                     ("Basic realm = \"\\f\\o\\o\"",                        "Basic",   [("realm", "foo")]);
                     ("Basic realm = \"foo\"",                              "Basic",   [("realm", "foo")]);
                     ("Basic realm=\"\\\"foo\\\"\"",                        "Basic",   [("realm", "\"foo\"")]);
                     ("Basic realm=\"foo\", bar=\"xyz\",, a=b,,,c=d",       "Basic",   [("realm", "foo"); ("bar", "xyz"); ("a", "b"); ("c", "d")]);
                     ("Basic bar=\"xyz\", realm=\"foo\"",                   "Basic",   [("realm", "foo"); ("bar", "xyz")]);
                     ("Basic realm=\"foo-\u00E4\"",                         "Basic",   [("realm", "foo-\u00E4")]);
                     ("Basic realm=\"foo-\u00A4\"",                         "Basic",   [("realm", "foo-\u00A4")]);
                     ("Basic realm=\"=?ISO-8859-1?Q?foo-=E4?=\"",           "Basic",   [("realm", "=?ISO-8859-1?Q?foo-=E4?=")]);
                     ("Newauth realm=\"newauth\"",                          "Newauth", [("realm", "newauth")]);
                     ("Basic foo=\"realm=nottherealm\", realm=\"basic\"",   "Basic",   [("foo", "realm=nottherealm"); ("realm", "basic")]);
                     ("Basic nottherealm=\"nottherealm\", realm=\"basic\"", "Basic",   [("nottherealm", "nottherealm"); ("realm", "basic")]);]
                     
        for (test, expectedScheme, expectedParams) in tests do
            match Parser.parse Challenge.Parser test with
            | Some result -> 
                result.Scheme |> should equal expectedScheme
                match result.DataOrParameters with
                | Choice2Of2 parameters-> parameters |> should equal (expectedParams |> Map.ofSeq)
                | _ -> failwith ("Parse failed for test: " + test)
            | _ -> failwith ("Parse failed for test: " + test)    
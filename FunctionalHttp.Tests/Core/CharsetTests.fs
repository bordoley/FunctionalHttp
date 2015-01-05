namespace FunctionalHttp.Core.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp.Core
open FunctionalHttp.Parsing

(*
module CharsetTests =
    [<Test>]
    let ``test parsing valid charsets`` () =
        let tests = [ 
            ("ISO-8859-1", Charset.ISO_8859_1); 
            ("iso-8859-1", Charset.ISO_8859_1);
            ("iSo-8859-1", Charset.ISO_8859_1);
            ("UTF-8", Charset.UTF_8); ]

        for (test, expected) in tests do
            match Parser.parse Charset.Parser test with
            | Some result -> result |> should equal expected
            | _ -> failwith ("Parse failed for test: " + test)

    [<Test>]
    let ``test parsing invalid charsets`` () =
        let tests = ["\"ISO-8859-1\""]

        for test in tests do
            match Parser.parse Charset.Parser test with
            | None -> ()
            | _ -> failwith ("Expected parsing to fail: " + test)
            *)
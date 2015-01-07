namespace FunctionalHttp.Parsing.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp.Parsing
open System

module CharStreamTests =
    [<Test>]
    let ``test CharStream.SubSequence() `` () =
        // Test the invariants
        (fun () -> CharStream.Empty.SubSequence(-1, 0) |> ignore) |> should throw typeof<ArgumentOutOfRangeException>
        (fun () -> CharStream.Empty.SubSequence(0, -1) |> ignore) |> should throw typeof<ArgumentOutOfRangeException>
        (fun () -> CharStream.Empty.SubSequence(1, 0) |> ignore) |> should throw typeof<ArgumentException>

        let testCase = "test string"
        let test = CharStream.Create(testCase);
        test.SubSequence(1, (test.Length - 1)).Length |> should equal (test.Length - 1)
        test.SubSequence(0) |> should equal test
        test.SubSequence(0,0) |> should equal CharStream.Empty

    [<Test>]
    let ``test CharStream.ToString() `` () =
        CharStream.Empty.ToString() |> should equal ""

        let testCase = "test string"
        CharStream.Create(testCase).SubSequence(1,5).ToString() |>  should equal (testCase.Substring(1,5))

    [<Test>]
    let ``test CharStream.Item() `` () =
        (fun () -> CharStream.Empty.Item -1 |> ignore) |> should throw typeof<ArgumentOutOfRangeException>
        (fun () -> CharStream.Empty.Item 0 |> ignore) |> should throw typeof<ArgumentOutOfRangeException>
        
        let expected = "test case"
        let test = CharStream.Create(expected)
        for i = 0 to expected.Length - 1 do
            test.Item i |> should equal (expected.Chars(i))
        
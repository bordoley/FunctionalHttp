namespace FunctionalHttp.Parsing.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp.Parsing
open System

module CharStreamTests =
    [<Test>]
    let ``test CharStream.GetSlice() `` () =
        // Test the invariants
        (fun () -> CharStream.Create("").[-1..0] |> ignore) |> should throw typeof<ArgumentOutOfRangeException>
        (fun () -> CharStream.Create("").[0..-1] |> ignore) |> should throw typeof<ArgumentOutOfRangeException>
        (fun () -> CharStream.Create("").[1..0] |> ignore) |> should throw typeof<ArgumentException>

        let testCase = "test string"
        let test = CharStream.Create(testCase);
        test.[1..(test.Length - 1)].Length |> should equal (test.Length - 1)
        test.[0..] |> should equal test
        test.[0..0] |> should equal <| CharStream.Create("")

    [<Test>]
    let ``test CharStream.ToString() `` () =
        CharStream.Create("").ToString() |> should equal ""

        let testCase = "test string"
        CharStream.Create(testCase).[1..5].ToString() |>  should equal (testCase.Substring(1,5))

    [<Test>]
    let ``test CharStream.Item() `` () =
        (fun () -> CharStream.Create("").Item -1 |> ignore) |> should throw typeof<ArgumentOutOfRangeException>
        (fun () -> CharStream.Create("").Item 0 |> ignore) |> should throw typeof<ArgumentOutOfRangeException>
        
        let expected = "test case"
        let test = CharStream.Create(expected)
        for i = 0 to expected.Length - 1 do
            test.Item i |> should equal (expected.Chars(i))
        
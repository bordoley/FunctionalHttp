namespace FunctionalHttp.Parsing.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp.Parsing

module CharStreamTests =
    [<Test>]
    let ``test CharInput.SubSequence()`` () =
        let testInput = CharStream("test string")

        testInput.SubSequence(0, testInput.Length) |> should equal testInput
        testInput.SubSequence(1,0) |> should sameAs CharStream.Empty
        
        let subseq = testInput.SubSequence(1,3)
        subseq.Length |> should equal 3
        subseq.Item 0 |> should equal 'e'

        let subseq = subseq.SubSequence(1,3)
        subseq.Length |> should equal 3
        subseq.Item 0 |> should equal 's'
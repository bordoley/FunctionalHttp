
namespace FunctionalHttp.Core.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp.Core
open FunctionalHttp.Parsing

open System

module UserAgentTests = 
    [<Test>]
    let testParse () =
        let testcase = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36"

        match CharStream(testcase) |> UserAgent.Parser with
        | Success (result, iNext, next) -> 
            Console.Write (string result)
        | Fail iFail ->
            Console.Write (iFail)
        | Eof -> 
            Console.Write ("eof")
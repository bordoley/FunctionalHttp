
namespace FunctionalHttp.Core.Tests

open NUnit.Framework
open FsUnit

open FunctionalHttp.Core
open Sparse

open System

module UserAgentTests = 
    [<Test>]
    let testParse () =
        let testcase = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36"

        match parse UserAgent.parser testcase with
        | Success (result, next) -> 
            Console.Write (string result)
        | Fail iFail ->
            Console.Write (iFail)
namespace FunctionalHttp.Tests

open FunctionalHttp.Core
open FunctionalHttp.Parsing
open NUnit.Framework
open System

module IPAddress = 
    [<Test>]
    let testIPv4Parse () =
        let result = parse IPv4Address.Parser "0.2.23.245"
        ()



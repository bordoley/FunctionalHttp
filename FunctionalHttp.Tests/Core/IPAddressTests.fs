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

    [<Test>]
    let testIPv6Parse () =
        //6( h16 ":" ) ls32
        //6( h16 “:” ) h16  “::”
        //5( h16 “:” ) h16  “::”
        //5( h16 “:” ) h16  “::”  h16

        //4( h16 “:” ) h16  “::”
        //4( h16 “:” ) h16  “::”  h16
        //4( h16 “:” ) h16  “::”  0(h16 “:”) ls32

        //3( h16 “:” ) h16  “::”
        //3( h16 “:” ) h16  “::”  h16
        //3( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        //3( h16 “:” ) h16  “::”  1(h16 “:”) ls32

        //2( h16 “:” ) h16  “::”
        //2( h16 “:” ) h16  “::”  h16
        //2( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        //2( h16 “:” ) h16  “::”  1(h16 “:”) ls32
        //2( h16 “:” ) h16  “::”  2(h16 “:”) ls32

        //1( h16 “:” ) h16  “::”
        //1( h16 “:” ) h16  “::”  h16
        //1( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        //1( h16 “:” ) h16  “::”  1(h16 “:”) ls32
        //1( h16 “:” ) h16  “::”  2(h16 “:”) ls32
        //1( h16 “:” ) h16  “::”  3(h16 “:”) ls32

        //0( h16 “:” ) h16  “::”
        //0( h16 “:” ) h16  “::”  h16
        //0( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        //0( h16 “:” ) h16  “::”  1(h16 “:”) ls32
        //0( h16 “:” ) h16  “::”  2(h16 “:”) ls32
        //0( h16 “:” ) h16  “::”  3(h16 “:”) ls32
        //0( h16 “:” ) h16  “::”  4(h16 “:”) ls32

        //“::” 5( h16 “:” ) ls32
        let result = parse IPv6Address.Parser "6666:6666::6666"
        ()



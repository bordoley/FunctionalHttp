namespace FunctionalHttp.Tests

open FunctionalHttp.Core
open Sparse
open NUnit.Framework
open FsUnit
open System

module IPAddress = 
    [<Test>]
    let testIPv4Parse () =
        let result = parse IPv4Address.Parser "0.2.23.245"
        ()

    [<Test>]
    let testIPv6Parse () =
        let testIPv6Parse test expected =
            match parse IPv6Address.Parser test with
            | Success (result, next) ->
                result |> should equal expected
            | _ -> failwith ("parsing failed for " + test)

        //6( h16 ":" )            0(h16 “:”) ls32
        testIPv6Parse "0000:1111:2222:3333:4444:5555:6666:7777" (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0x66667777u))

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



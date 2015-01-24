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
        testIPv6Parse "0000:1111:2222:3333:4444:5555:6666:7777"       (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0x66667777u))
        testIPv6Parse "0000:1111:2222:3333:4444:5555:255.255.255.255" (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0xFFFFFFFFu))

        //6( h16 “:” ) h16  “::”
        testIPv6Parse "0000:1111:2222:3333:4444:5555:6666::"          (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0x66660000u))

        //5( h16 “:” ) h16  “::”
        testIPv6Parse "0000:1111:2222:3333:4444:5555::"               (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0x00000000u))

        //5( h16 “:” ) h16  “::”  h16
        testIPv6Parse "0000:1111:2222:3333:4444:5555::6666"           (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0x00006666u))

        //4( h16 “:” ) h16  “::”  
        testIPv6Parse "0000:1111:2222:3333:4444::"                    (IPv6Address(0x00001111u, 0x22223333u, 0x44440000u, 0x00000000u))

        //4( h16 “:” ) h16  “::”  h16
        testIPv6Parse "0000:1111:2222:3333:4444::5555"                (IPv6Address(0x00001111u, 0x22223333u, 0x44440000u, 0x00005555u))

        //4( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        testIPv6Parse "0000:1111:2222:3333:4444::5555:6666"           (IPv6Address(0x00001111u, 0x22223333u, 0x44440000u, 0x55556666u))
        testIPv6Parse "0000:1111:2222:3333:4444::255.255.255.255"     (IPv6Address(0x00001111u, 0x22223333u, 0x44440000u, 0xFFFFFFFFu))

        //3( h16 “:” ) h16  “::”
        testIPv6Parse "0000:1111:2222:3333::"                         (IPv6Address(0x00001111u, 0x22223333u, 0x00000000u, 0x00000000u))
        
        //3( h16 “:” ) h16  “::”  h16
        testIPv6Parse "0000:1111:2222:3333::4444"                     (IPv6Address(0x00001111u, 0x22223333u, 0x00000000u, 0x00004444u))
        
        //3( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        testIPv6Parse "0000:1111:2222:3333::4444:5555"                (IPv6Address(0x00001111u, 0x22223333u, 0x00000000u, 0x44445555u))
        testIPv6Parse "0000:1111:2222:3333::255.255.255.255"          (IPv6Address(0x00001111u, 0x22223333u, 0x00000000u, 0xFFFFFFFFu))

        //3( h16 “:” ) h16  “::”  1(h16 “:”) ls32
        testIPv6Parse "0000:1111:2222:3333::4444:5555:6666"           (IPv6Address(0x00001111u, 0x22223333u, 0x00004444u, 0x55556666u))
        testIPv6Parse "0000:1111:2222:3333::4444:255.255.255.255"     (IPv6Address(0x00001111u, 0x22223333u, 0x00004444u, 0xFFFFFFFFu))

        //2( h16 “:” ) h16  “::”
        testIPv6Parse "0000:1111:2222::"                              (IPv6Address(0x00001111u, 0x22220000u, 0x00000000u, 0x00000000u))

        //2( h16 “:” ) h16  “::”  h16
        testIPv6Parse "0000:1111:2222::3333"                          (IPv6Address(0x00001111u, 0x22220000u, 0x00000000u, 0x00003333u))

        //2( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        testIPv6Parse "0000:1111:2222::3333:4444"                     (IPv6Address(0x00001111u, 0x22220000u, 0x00000000u, 0x33334444u))
        testIPv6Parse "0000:1111:2222::255.255.255.255"               (IPv6Address(0x00001111u, 0x22220000u, 0x00000000u, 0xFFFFFFFFu))

        //2( h16 “:” ) h16  “::”  1(h16 “:”) ls32
        testIPv6Parse "0000:1111:2222::3333:4444:5555"                (IPv6Address(0x00001111u, 0x22220000u, 0x00003333u, 0x44445555u))
        testIPv6Parse "0000:1111:2222::3333:255.255.255.255"          (IPv6Address(0x00001111u, 0x22220000u, 0x00003333u, 0xFFFFFFFFu))

        //2( h16 “:” ) h16  “::”  2(h16 “:”) ls32
        testIPv6Parse "0000:1111:2222::3333:4444:5555:6666"           (IPv6Address(0x00001111u, 0x22220000u, 0x33334444u, 0x55556666u))
        testIPv6Parse "0000:1111:2222::3333:4444:255.255.255.255"     (IPv6Address(0x00001111u, 0x22220000u, 0x33334444u, 0xFFFFFFFFu))

        //1( h16 “:” ) h16  “::”
        testIPv6Parse "0000:1111::"                                   (IPv6Address(0x00001111u, 0x00000000u, 0x00000000u, 0x00000000u))

        //1( h16 “:” ) h16  “::”  h16
        testIPv6Parse "0000:1111::2222"                               (IPv6Address(0x00001111u, 0x00000000u, 0x00000000u, 0x00002222u))
        
        //1( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        testIPv6Parse "0000:1111::2222:3333"                          (IPv6Address(0x00001111u, 0x00000000u, 0x00000000u, 0x22223333u))
        testIPv6Parse "0000:1111::255.255.255.255"                    (IPv6Address(0x00001111u, 0x00000000u, 0x00000000u, 0xFFFFFFFFu))

        //1( h16 “:” ) h16  “::”  1(h16 “:”) ls32
        testIPv6Parse "0000:1111::2222:3333:4444"                     (IPv6Address(0x00001111u, 0x00000000u, 0x00002222u, 0x33334444u))
        testIPv6Parse "0000:1111::2222:255.255.255.255"               (IPv6Address(0x00001111u, 0x00000000u, 0x00002222u, 0xFFFFFFFFu))

        //1( h16 “:” ) h16  “::”  2(h16 “:”) ls32
        testIPv6Parse "0000:1111::2222:3333:4444:5555"                (IPv6Address(0x00001111u, 0x00000000u, 0x22223333u, 0x44445555u))
        testIPv6Parse "0000:1111::2222:3333:255.255.255.255"          (IPv6Address(0x00001111u, 0x00000000u, 0x22223333u, 0xFFFFFFFFu))

        //1( h16 “:” ) h16  “::”  3(h16 “:”) ls32
        testIPv6Parse "0000:1111::2222:3333:4444:5555:6666"           (IPv6Address(0x00001111u, 0x00002222u, 0x33334444u, 0x55556666u))
        testIPv6Parse "0000:1111::2222:3333:4444:255.255.255.255"     (IPv6Address(0x00001111u, 0x00002222u, 0x33334444u, 0xFFFFFFFFu))

        //0( h16 “:” ) h16  “::”
        testIPv6Parse "1111::"                                        (IPv6Address(0x11110000u, 0x00000000u, 0x00000000u, 0x00000000u))

        //0( h16 “:” ) h16  “::”  h16
        testIPv6Parse "1111::2222"                                    (IPv6Address(0x11110000u, 0x00000000u, 0x00000000u, 0x00002222u))

        //0( h16 “:” ) h16  “::”  0(h16 “:”) ls32
        testIPv6Parse "1111::2222:3333"                               (IPv6Address(0x11110000u, 0x00000000u, 0x00000000u, 0x22223333u))
        testIPv6Parse "1111::255.255.255.255"                         (IPv6Address(0x11110000u, 0x00000000u, 0x00000000u, 0xFFFFFFFFu))

        //0( h16 “:” ) h16  “::”  1(h16 “:”) ls32
        testIPv6Parse "1111::2222:3333:4444"                          (IPv6Address(0x11110000u, 0x00000000u, 0x00002222u, 0x33334444u))
        testIPv6Parse "1111::2222:255.255.255.255"                    (IPv6Address(0x11110000u, 0x00000000u, 0x00002222u, 0xFFFFFFFFu))

        //0( h16 “:” ) h16  “::”  2(h16 “:”) ls32
        testIPv6Parse "1111::2222:3333:4444:5555"                     (IPv6Address(0x11110000u, 0x00000000u, 0x22223333u, 0x44445555u))
        testIPv6Parse "1111::2222:3333:255.255.255.255"               (IPv6Address(0x11110000u, 0x00000000u, 0x22223333u, 0xFFFFFFFFu))

        //0( h16 “:” ) h16  “::”  3(h16 “:”) ls32
        testIPv6Parse "1111::2222:3333:4444:5555:6666"                (IPv6Address(0x11110000u, 0x00002222u, 0x33334444u, 0x55556666u))
        testIPv6Parse "1111::2222:3333:4444:255.255.255.255"          (IPv6Address(0x11110000u, 0x00002222u, 0x33334444u, 0xFFFFFFFFu))

        //0( h16 “:” ) h16  “::”  4(h16 “:”) ls32
        testIPv6Parse "1111::2222:3333:4444:5555:6666:7777"           (IPv6Address(0x11110000u, 0x22223333u, 0x44445555u, 0x66667777u))
        testIPv6Parse "1111::2222:3333:4444:5555:255.255.255.255"     (IPv6Address(0x11110000u, 0x22223333u, 0x44445555u, 0xFFFFFFFFu))

        //“::” 0*5( h16 “:” ) ls32 / h16
        testIPv6Parse "::"                                            (IPv6Address(0x00000000u, 0x00000000u, 0x00000000u, 0x00000000u))
        testIPv6Parse "::1111"                                        (IPv6Address(0x00000000u, 0x00000000u, 0x00000000u, 0x00001111u))
        
        testIPv6Parse "::1111:2222"                                   (IPv6Address(0x00000000u, 0x00000000u, 0x00000000u, 0x11112222u))
        testIPv6Parse "::255.255.255.255"                             (IPv6Address(0x00000000u, 0x00000000u, 0x00000000u, 0xFFFFFFFFu))

        testIPv6Parse "::1111:2222:3333"                              (IPv6Address(0x00000000u, 0x00000000u, 0x00001111u, 0x22223333u))
        testIPv6Parse "::1111:255.255.255.255"                        (IPv6Address(0x00000000u, 0x00000000u, 0x00001111u, 0xFFFFFFFFu))

        testIPv6Parse "::1111:2222:3333:4444"                         (IPv6Address(0x00000000u, 0x00000000u, 0x11112222u, 0x33334444u))
        testIPv6Parse "::1111:2222:255.255.255.255"                   (IPv6Address(0x00000000u, 0x00000000u, 0x11112222u, 0xFFFFFFFFu))

        testIPv6Parse "::1111:2222:3333:4444:5555"                    (IPv6Address(0x00000000u, 0x00001111u, 0x22223333u, 0x44445555u))
        testIPv6Parse "::1111:2222:3333:255.255.255.255"              (IPv6Address(0x00000000u, 0x00001111u, 0x22223333u, 0xFFFFFFFFu))

        testIPv6Parse "::1111:2222:3333:4444:5555:6666"               (IPv6Address(0x00000000u, 0x11112222u, 0x33334444u, 0x55556666u))
        testIPv6Parse "::1111:2222:3333:4444:255.255.255.255"         (IPv6Address(0x00000000u, 0x11112222u, 0x33334444u, 0xFFFFFFFFu))

        testIPv6Parse "::1111:2222:3333:4444:5555:6666:7777"          (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0x66667777u))
        testIPv6Parse "::1111:2222:3333:4444:5555:255.255.255.255"    (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0xFFFFFFFFu))

        testIPv6Parse "1:22:333:4444:1:22:33:4444"                    (IPv6Address(0x00010022u, 0x03334444u, 0x00010022u, 0x03334444u))

        match parse IPv4Address.Parser "0000:1111:2222:3333:4444:5555::255.255.255.255" with
        | Success (result, next) ->
                result |> should equal (IPv6Address(0x00001111u, 0x22223333u, 0x44445555u, 0x00000255u))
                next |> should equal 35
        | _ -> failwith ("parsing failed")
        ()



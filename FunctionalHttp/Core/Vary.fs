namespace FunctionalHttp.Core
open System

type Vary = 
    private
    | Headers of Header seq
    | Any

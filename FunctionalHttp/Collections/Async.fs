namespace FunctionalHttp.Collections

// based on https://github.com/fsprojects/fsharpx/blob/master/src/FSharpx.Core/ComputationExpressions/Monad.fs
module internal Async =
    let bind f m  = async.Bind (m, f)

    let map f m =
        let inline ret x = async.Return (f x)
        async.Bind (m, ret)
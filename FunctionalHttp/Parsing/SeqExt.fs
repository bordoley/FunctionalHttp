namespace FunctionalHttp

module internal SeqExt =
    let lastIfPresent seq =
        let result = ref None
        for x in seq do
            result := Some x         
        !result


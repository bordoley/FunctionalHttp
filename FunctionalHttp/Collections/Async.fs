namespace FunctionalHttp.Collections

module internal Async =
    let result r = async { return r }

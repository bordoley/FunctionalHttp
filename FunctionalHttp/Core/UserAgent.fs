namespace FunctionalHttp.Core

type UserAgent = 
    private | UserAgent of Product*(Choice<Product,Comment> seq)


namespace FunctionalHttp

type UserAgent = 
    private | UserAgent of Product*(Choice<Product,Comment> seq)


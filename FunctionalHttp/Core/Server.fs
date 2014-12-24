namespace FunctionalHttp.Core

type Server = 
    private | Server of Product*(Choice<Product,Comment> seq)

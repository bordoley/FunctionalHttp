namespace FunctionalHttp

type Server = 
    private | Server of Product*(Choice<Product,Comment> seq)

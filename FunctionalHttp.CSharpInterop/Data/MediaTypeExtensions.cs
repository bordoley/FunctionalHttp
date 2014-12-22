using FunctionalHttp;

using Microsoft.FSharp.Core;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunctionalHttp.Interop
{
    public static class MediaTypeExtensions
    {
        public static bool TryGetCharset(this MediaType This, out Charset charset)
        {
            if (FSharpOption<Charset>.get_IsSome(This.Charset))
            {
                charset = This.Charset.Value;
                return true;
            }

            charset = null;
            return false;
        }
    }
}

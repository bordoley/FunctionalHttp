using FunctionalHttp.Collections;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;

namespace FunctionalHttp.Core.Interop
{
    public static class ContentInfoExtensionsCSharp
    {
        public static ContentInfo With(
            this ContentInfo This, 
            IEnumerable<ContentCoding> encodings = null,
            IEnumerable<LanguageTag> languages = null,
            int? length = null, 
            Uri location = null, 
            MediaType mediaType = null)
        {
            return ContentInfoModule.With(
                This,
                encodings.ToFSharpOption(),
                languages.ToFSharpOption(),
                length.ToFSharpOption(),
                location.ToFSharpOption(),
                mediaType.ToFSharpOption());
        }

        public static ContentInfo Without(
            this ContentInfo This,
            bool encodings = false,
            bool languages = false,
            bool length = false, 
            bool location = false, 
            bool mediaType = false)
        {
            return ContentInfoModule.Without(This, encodings, languages, length, location, mediaType);
        }
    }
}

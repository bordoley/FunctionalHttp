using FunctionalHttp.Collections;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;

namespace FunctionalHttp.Core
{
    public static class ContentInfoExtensions
    {
        public static ContentInfo With(
            this ContentInfo This, 
            IEnumerable<ContentCoding> encodings = null,
            IEnumerable<LanguageTag> languages = null,
            UInt64? length = null, 
            Uri location = null, 
            MediaType mediaType = null,
            FSharpChoice<ByteContentRange, OtherContentRange> range = null)
        {
            return ContentInfoModule.With(
                encodings.ToFSharpOption(),
                languages.ToFSharpOption(),
                length.ToFSharpOption(),
                location.ToFSharpOption(),
                mediaType.ToFSharpOption(),
                range.ToFSharpOption(),
                This);
        }

        public static ContentInfo Without(
            this ContentInfo This,
            bool encodings = false,
            bool languages = false,
            bool length = false, 
            bool location = false, 
            bool mediaType = false,
            bool range = false)
        {
            return ContentInfoModule.Without(encodings, languages, length, location, mediaType, range, This);
        }
    }
}

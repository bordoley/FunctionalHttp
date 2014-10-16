using Microsoft.FSharp.Core;
using System;

namespace FunctionalHttp
{
    public static class ContentInfoExtensionsCSharp
    {
        public static ContentInfo With(this ContentInfo This,  int? length = null, Uri location = null, MediaRange mediaRange = null)
        {
            return ContentInfo.CreateInternal(
                length != null ? FSharpOption<int>.Some(length.Value) : This.length,
                location != null ? FSharpOption<Uri>.Some(location) : This.Location,
                mediaRange != null ? FSharpOption<MediaRange>.Some(mediaRange) : This.MediaRange);
        }

        public static ContentInfo Without(this ContentInfo This, bool length = false, bool location = false, bool mediaRange = false)
        {
            return ContentInfo.CreateInternal(
                length ? FSharpOption<int>.None : This.length,
                location ? FSharpOption<Uri>.None : This.Location,
                mediaRange ? FSharpOption<MediaRange>.None : This.MediaRange);
        }
    }
}

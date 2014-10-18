using Microsoft.FSharp.Core;
using System;

namespace FunctionalHttp.Interop
{
    public static class ContentInfoExtensions
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

        public static bool TryGetLength(this ContentInfo This, out int length)
        {
            if(OptionModule.IsSome(This.Length))
            {
                length = This.Length.Value;
                return true;
            }

            length = -1;
            return false;
        }

        public static bool TryGetLocation(this ContentInfo This, out Uri location)
        {
            if (OptionModule.IsSome(This.Location))
            {
                location = This.Location.Value;
                return true;
            }

            location = null;
            return false;
        }

        public static bool TryGetMediaRange(this ContentInfo This, out MediaRange mediaRange)
        {
            if (OptionModule.IsSome(This.MediaRange))
            {
                mediaRange = This.MediaRange.Value;
                return true;
            }

            mediaRange = null;
            return false;
        }
    }
}

using Microsoft.FSharp.Core;
using System;

namespace FunctionalHttp.Interop
{
    public static class ContentInfoExtensions
    {
        public static ContentInfo With(this ContentInfo This, int? length = null, Uri location = null, MediaType mediaType = null)
        {
            return ContentInfo.CreateInternal(
                length != null ? FSharpOption<int>.Some(length.Value) : This.length,
                location != null ? FSharpOption<Uri>.Some(location) : This.Location,
                mediaType != null ? FSharpOption<MediaType>.Some(mediaType) : This.MediaType);
        }

        public static ContentInfo Without(this ContentInfo This, bool length = false, bool location = false, bool mediaType = false)
        {
            return ContentInfo.CreateInternal(
                length ? FSharpOption<int>.None : This.length,
                location ? FSharpOption<Uri>.None : This.Location,
                mediaType ? FSharpOption<MediaType>.None : This.MediaType);
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

        public static bool TryGetMediaType(this ContentInfo This, out MediaType mediaType)
        {
            if (OptionModule.IsSome(This.MediaType))
            {
                mediaType = This.MediaType.Value;
                return true;
            }

            mediaType = null;
            return false;
        }
    }
}

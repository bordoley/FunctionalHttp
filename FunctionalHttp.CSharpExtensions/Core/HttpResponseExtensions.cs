using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;

using FunctionalHttp;

namespace FunctionalHttp.Interop
{
    public static class HttpResponseExtensionsCSharp
    {
        public static HttpResponse<TResp> With<TResp>(
            this HttpResponse<TResp> This,
            Status status = null,
            ContentInfo contentInfo = null,
            TimeSpan? age = null,
            IEnumerable<CacheDirective> cacheControl = null,
            DateTimeOffset? expires = null,
            Uri location = null)
        {
            return new HttpResponse<TResp>(
               status != null ? status : This.Status,
               This.Entity,
               This.Id,
               contentInfo != null ? contentInfo : This.ContentInfo,
               age != null ? FSharpOption<TimeSpan>.Some(age.Value) : This.Age,
               SetModule.OfSeq<CacheDirective>(cacheControl != null ? cacheControl : This.CacheControl),
               expires != null ? FSharpOption<DateTimeOffset>.Some(expires.Value) : This.Expires,
               location != null ? FSharpOption<Uri>.Some(location) : This.Location);
        }

        public static HttpResponse<TNew> With<TResp, TNew>(
            this HttpResponse<TResp> This,
            TNew entity, 
            Status status = null,
            ContentInfo contentInfo = null,
            TimeSpan? age = null,
            IEnumerable<CacheDirective> cacheControl = null,
            DateTimeOffset? expires = null,
            Uri location = null)
        {
            return new HttpResponse<TNew>(
               status != null ? status : This.Status,
               FSharpOption<TNew>.Some(entity),
               This.Id,
               contentInfo != null ? contentInfo : This.ContentInfo,
               age != null ? FSharpOption<TimeSpan>.Some(age.Value) : This.Age,
               SetModule.OfSeq<CacheDirective>(cacheControl != null ? cacheControl : This.CacheControl),
               expires != null ? FSharpOption<DateTimeOffset>.Some(expires.Value) : This.Expires,
               location != null ? FSharpOption<Uri>.Some(location) : This.Location);
        }

        public static HttpResponse<TResp> Without<TResp>(
            this HttpResponse<TResp> This,
            bool contentInfo = false,
            bool age = false,
            bool cacheControl = false,
            bool expires = false,
            bool location = false)
        {
            return new HttpResponse<TResp>(
                This.Status,
                This.Entity,
                This.Id,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                age ? FSharpOption<TimeSpan>.None : This.Age,
                SetModule.OfSeq<CacheDirective>(cacheControl ? SeqModule.Empty<CacheDirective>() : This.CacheControl),
                expires ? FSharpOption<DateTimeOffset>.None : This.Expires,
                location ? FSharpOption<Uri>.None : This.Location);
        }

        public static HttpResponse<TNew> WithoutEntity<TResp, TNew>(
            this HttpResponse<TResp> This,
            bool contentInfo = false,
            bool age = false,
            bool cacheControl = false,
            bool expires = false,
            bool location = false)
        {
            return new HttpResponse<TNew>(
                This.Status,
                FSharpOption<TNew>.None,
                This.Id,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                age ? FSharpOption<TimeSpan>.None : This.Age,
                SetModule.OfSeq<CacheDirective>(cacheControl ? SeqModule.Empty<CacheDirective>() : This.CacheControl),
                expires ? FSharpOption<DateTimeOffset>.None : This.Expires,
                location ? FSharpOption<Uri>.None : This.Location);
        }

        public static bool TryGetEntity<TResp>(this HttpResponse<TResp> This, out TResp entity)
            where TResp : class
        {
            if (OptionModule.IsSome(This.Entity))
            {
                entity = This.Entity.Value;
                return true;
            }

            entity = null;
            return false;
        }

        public static bool TryGetAge<TResp>(this HttpResponse<TResp> This, out TimeSpan age)
        {
            if (OptionModule.IsSome(This.Age))
            {
                age = This.Age.Value;
                return true;
            }

            age = TimeSpan.MinValue;
            return false;
        }

        public static bool TryGetExpires<TResp>(this HttpResponse<TResp> This, out DateTimeOffset expires)
        {
            if (OptionModule.IsSome(This.Expires))
            {
                expires = This.Expires.Value;
                return true;
            }

            expires = DateTimeOffset.MinValue;
            return false;
        }

        public static bool TryGetLocation<TResp>(this HttpResponse<TResp> This, out Uri location)
        {
            if (OptionModule.IsSome(This.Location))
            {
                location = This.Location.Value;
                return true;
            }

            location = null;
            return false;
        }
    }
}

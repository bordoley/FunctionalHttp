﻿using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;

namespace FunctionalHttp
{
    public static class HttpResponseExtensionsCSharp
    {
        public static HttpResponse<TResp> With<TResp>(
            this HttpResponse<TResp> This,
            Status status = null,
            ContentInfo contentInfo = null,
            TimeSpan? age = null,
            IEnumerable<CacheDirective> cacheDirectives = null,
            DateTimeOffset? expires = null,
            Uri location = null)
        {
            return new HttpResponse<TResp>(
               status != null ? status : This.Status,
               This.Entity,
               This.Id,
               contentInfo != null ? contentInfo : This.ContentInfo,
               age != null ? FSharpOption<TimeSpan>.Some(age.Value) : This.Age,
               SetModule.OfSeq<CacheDirective>(cacheDirectives != null ? cacheDirectives : This.CacheDirectives),
               expires != null ? FSharpOption<DateTimeOffset>.Some(expires.Value) : This.Expires,
               location != null ? FSharpOption<Uri>.Some(location) : This.Location);
        }

        public static HttpResponse<TNew> With<TResp, TNew>(
            this HttpResponse<TResp> This,
            TNew entity, 
            Status status = null,
            ContentInfo contentInfo = null,
            TimeSpan? age = null,
            IEnumerable<CacheDirective> cacheDirectives = null,
            DateTimeOffset? expires = null,
            Uri location = null)
        {
            return new HttpResponse<TNew>(
               status != null ? status : This.Status,
               FSharpOption<TNew>.Some(entity),
               This.Id,
               contentInfo != null ? contentInfo : This.ContentInfo,
               age != null ? FSharpOption<TimeSpan>.Some(age.Value) : This.Age,
               SetModule.OfSeq<CacheDirective>(cacheDirectives != null ? cacheDirectives : This.CacheDirectives),
               expires != null ? FSharpOption<DateTimeOffset>.Some(expires.Value) : This.Expires,
               location != null ? FSharpOption<Uri>.Some(location) : This.Location);
        }

        public static HttpResponse<TResp> Without<TResp>(
            this HttpResponse<TResp> This,
            bool contentInfo = false,
            bool age = false,
            bool cacheDirectives = false,
            bool expires = false,
            bool location = false)
        {
            return new HttpResponse<TResp>(
                This.Status,
                This.Entity,
                This.Id,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                age ? FSharpOption<TimeSpan>.None : This.Age,
                SetModule.OfSeq<CacheDirective>(cacheDirectives ? SeqModule.Empty<CacheDirective>() : This.CacheDirectives),
                expires ? FSharpOption<DateTimeOffset>.None : This.Expires,
                location ? FSharpOption<Uri>.None : This.Location);
        }

        public static HttpResponse<TNew> WithoutEntity<TResp, TNew>(
            this HttpResponse<TResp> This,
            bool contentInfo = false,
            bool age = false,
            bool cacheDirectives = false,
            bool expires = false,
            bool location = false)
        {
            return new HttpResponse<TNew>(
                This.Status,
                FSharpOption<TNew>.None,
                This.Id,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                age ? FSharpOption<TimeSpan>.None : This.Age,
                SetModule.OfSeq<CacheDirective>(cacheDirectives ? SeqModule.Empty<CacheDirective>() : This.CacheDirectives),
                expires ? FSharpOption<DateTimeOffset>.None : This.Expires,
                location ? FSharpOption<Uri>.None : This.Location);
        }
    }
}

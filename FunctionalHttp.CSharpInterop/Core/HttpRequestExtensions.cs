using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using FunctionalHttp;

namespace FunctionalHttp.Interop
{
    public static class HttpRequestExtensionsCSharp
    {
        public static HttpRequest<TReq> With<TReq>(
            this HttpRequest<TReq> This,    
            Challenge authorization = null,
            IEnumerable<CacheDirective> cacheControl = null,
            ContentInfo contentInfo = null,
            bool? expectContinue = null,
            IEnumerable<Tuple<Header, object>> headers = null,
            Guid? id = null,
            Method meth = null, 
            IEnumerable<CacheDirective> pragma = null,
            RequestPreconditions preconditions = null,
            RequestPreferences preferences = null,
            Challenge proxyAuthorization = null,
            Uri referer = null,
            Uri uri = null,
            UserAgent userAgent = null,
            HttpVersion version = null)
        {
            return HttpRequestModule.With<TReq, TReq>(
                This,
                OptionModule.OfNull(authorization),
                OptionModule.OfNull(cacheControl),
                OptionModule.OfNull(contentInfo),
                This.Entity,
                OptionModule.OfNullable(expectContinue),
                headers != null ? 
                    FSharpOption<FSharpMap<Header,object>>.Some(MapModule.OfSeq<Header, object>(headers)) : 
                    FSharpOption<FSharpMap<Header,object>>.None,
                OptionModule.OfNullable(id),
                OptionModule.OfNull(meth), 
                OptionModule.OfNull(pragma),
                OptionModule.OfNull(preconditions), 
                OptionModule.OfNull(preferences),
                OptionModule.OfNull(proxyAuthorization),
                OptionModule.OfNull(referer),
                OptionModule.OfNull(uri),
                OptionModule.OfNull(userAgent),
                OptionModule.OfNull(version));
        }

        public static HttpRequest<TNew> With<TReq, TNew>(
            this HttpRequest<TReq> This,
            TNew entity, 
            Challenge authorization = null,
            IEnumerable<CacheDirective> cacheControl = null,
            ContentInfo contentInfo = null,
            bool? expectContinue = null,
            IEnumerable<Tuple<Header, object>> headers = null,
            Guid? id = null,
            Method meth = null,
            IEnumerable<CacheDirective> pragma = null,
            RequestPreconditions preconditions = null,
            RequestPreferences preferences = null,
            Challenge proxyAuthorization = null,
            Uri referer = null,
            Uri uri = null,
            UserAgent userAgent = null,
            HttpVersion version = null)
        {
            return HttpRequestModule.With<TReq, TNew>(
                This,
                OptionModule.OfNull(authorization),
                OptionModule.OfNull(cacheControl),
                OptionModule.OfNull(contentInfo),
                OptionModule.OfNull(entity),
                OptionModule.OfNullable(expectContinue),
                headers != null ? 
                FSharpOption<FSharpMap<Header,object>>.Some(MapModule.OfSeq<Header, object>(headers)) : 
                FSharpOption<FSharpMap<Header,object>>.None,
                OptionModule.OfNullable(id),
                OptionModule.OfNull(meth), 
                OptionModule.OfNull(pragma),
                OptionModule.OfNull(preconditions), 
                OptionModule.OfNull(preferences),
                OptionModule.OfNull(proxyAuthorization),
                OptionModule.OfNull(referer),
                OptionModule.OfNull(uri),
                OptionModule.OfNull(userAgent),
                OptionModule.OfNull(version));
        }

        public static HttpRequest<TReq> Without<TReq>(
            this HttpRequest<TReq> This,
            bool authorization = false,
            bool cacheControl = false,
            bool contentInfo = false, 
            bool headers = false,
            bool pragma = false,
            bool preconditions = false,
            bool preferences = false,
            bool proxyAuthorization = false,
            bool referer = false,
            bool userAgent = false)
        {
            return HttpRequestModule.Without<TReq, TReq>(
                This, 
                This.Entity, 
                authorization, 
                cacheControl, 
                contentInfo, 
                headers,
                pragma, 
                preconditions, 
                preferences, 
                proxyAuthorization, 
                referer, 
                userAgent);
        }

        public static HttpRequest<TNew> WithoutEntity<TReq, TNew>(
            this HttpRequest<TReq> This, 
            bool authorization = false, 
            bool cacheControl = false,
            bool contentInfo = false,
            bool headers = false,
            bool pragma = false,
            bool preconditions = false,
            bool preferences = false,
            bool proxyAuthorization = false,
            bool referer = false,
            bool userAgent = false)
        {
            return HttpRequestModule.Without<TReq, TNew>(
                This, 
                FSharpOption<TNew>.None, 
                authorization, 
                cacheControl, 
                contentInfo, 
                headers,
                pragma, 
                preconditions, 
                preferences, 
                proxyAuthorization, 
                referer, 
                userAgent);
        }
    }
}

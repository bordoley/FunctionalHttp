using FunctionalHttp.Collections;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

using FunctionalHttp;

namespace FunctionalHttp.Core.Interop
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
                authorization.ToFSharpOption(),
                cacheControl.ToFSharpOption(),
                contentInfo.ToFSharpOption(),
                This.Entity,
                expectContinue.ToFSharpOption(),
                headers.ToFSharpOption().Select(MapModule.OfSeq),
                id.ToFSharpOption(),
                meth.ToFSharpOption(), 
                pragma.ToFSharpOption(),
                preconditions.ToFSharpOption(), 
                preferences.ToFSharpOption(),
                proxyAuthorization.ToFSharpOption(),
                referer.ToFSharpOption(),
                uri.ToFSharpOption(),
                userAgent.ToFSharpOption(),
                version.ToFSharpOption());
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
                authorization.ToFSharpOption(),
                cacheControl.ToFSharpOption(),
                contentInfo.ToFSharpOption(),
                entity.ToFSharpOption(),
                expectContinue.ToFSharpOption(),
                headers.ToFSharpOption().Select(MapModule.OfSeq),
                id.ToFSharpOption(),
                meth.ToFSharpOption(), 
                pragma.ToFSharpOption(),
                preconditions.ToFSharpOption(), 
                preferences.ToFSharpOption(),
                proxyAuthorization.ToFSharpOption(),
                referer.ToFSharpOption(),
                uri.ToFSharpOption(),
                userAgent.ToFSharpOption(),
                version.ToFSharpOption());
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

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
            return new HttpRequest<TReq> (
                authorization != null ? FSharpOption<Challenge>.Some(authorization) : This.Authorization,
                cacheControl != null ? SetModule.OfSeq<CacheDirective>(cacheControl) : This.CacheControl,
                contentInfo ?? This.ContentInfo,
                This.Entity, 
                expectContinue != null ? expectContinue.Value : This.ExpectContinue,
                headers != null ? MapModule.OfSeq<Header, object>(headers) : This.Headers,
                id != null ? id.Value : This.Id,
                meth ?? This.Method,
                pragma != null ? SetModule.OfSeq <CacheDirective>(pragma) : This.Pragma,
                preconditions ?? This.Preconditions,
                preferences ?? This.Preferences,
                proxyAuthorization != null ? FSharpOption<Challenge>.Some(proxyAuthorization) : This.ProxyAuthorization,
                referer != null ? FSharpOption<Uri>.Some(referer) : This.Referer,
                uri ?? This.Uri,
                userAgent != null ? FSharpOption<UserAgent>.Some(userAgent) : This.UserAgent,
                version ?? This.Version);
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
            Contract.Requires(entity != null);

            return new HttpRequest<TNew>(
                authorization != null ? FSharpOption<Challenge>.Some(authorization) : This.Authorization,
                cacheControl != null ? SetModule.OfSeq<CacheDirective>(cacheControl) : This.CacheControl,
                contentInfo ?? This.ContentInfo,
                FSharpOption<TNew>.Some(entity),
                expectContinue != null ? expectContinue.Value : This.ExpectContinue,
                headers != null ? MapModule.OfSeq<Header, object>(headers) : This.Headers,
                id != null ? id.Value : This.Id,
                meth ?? This.Method,
                pragma != null ? SetModule.OfSeq<CacheDirective>(pragma) : This.Pragma,
                preconditions ?? This.Preconditions,
                preferences ?? This.Preferences,
                proxyAuthorization != null ? FSharpOption<Challenge>.Some(proxyAuthorization) : This.ProxyAuthorization,
                referer != null ? FSharpOption<Uri>.Some(referer) : This.Referer,
                uri ?? This.Uri,
                userAgent != null ? FSharpOption<UserAgent>.Some(userAgent) : This.UserAgent,
                version ?? This.Version);
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
            return new HttpRequest<TReq>(
                authorization ? FSharpOption<Challenge>.None : This.Authorization,
                cacheControl ? SetModule.Empty<CacheDirective>() : This.CacheControl,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                This.Entity,
                This.ExpectContinue,
                headers ? MapModule.Empty<Header, object>() : This.Headers,
                This.Id,
                This.Method,
                pragma ? SetModule.Empty<CacheDirective>() : This.Pragma,
                preconditions ? RequestPreconditions.None : This.Preconditions,
                preferences ? RequestPreferences.None : This.Preferences,
                proxyAuthorization ? FSharpOption<Challenge>.None : This.ProxyAuthorization,
                referer ? FSharpOption<Uri>.None : This.Referer,
                This.Uri,
                userAgent ? FSharpOption<UserAgent>.None : This.UserAgent,
                This.Version);
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
            return new HttpRequest<TNew>( 
                authorization ? FSharpOption<Challenge>.None : This.Authorization,
                cacheControl ? SetModule.Empty<CacheDirective>() : This.CacheControl,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                FSharpOption<TNew>.None,
                This.ExpectContinue,
                headers ? MapModule.Empty<Header, object>() : This.Headers,
                This.Id,
                This.Method,
                pragma ? SetModule.Empty<CacheDirective>() : This.Pragma,
                preconditions ? RequestPreconditions.None : This.Preconditions,
                preferences ? RequestPreferences.None : This.Preferences,
                proxyAuthorization ? FSharpOption<Challenge>.None : This.ProxyAuthorization,
                referer ? FSharpOption<Uri>.None : This.Referer,
                This.Uri,
                userAgent ? FSharpOption<UserAgent>.None : This.UserAgent,
                This.Version);
        }
    }
}

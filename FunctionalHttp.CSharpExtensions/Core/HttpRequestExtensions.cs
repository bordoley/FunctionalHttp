using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FunctionalHttp.Interop
{
    public static class HttpRequestExtensions
    {
        public static HttpRequest<TReq> With<TReq>(
            this HttpRequest<TReq> This,    
            ChallengeMessage authorization = null,
            IEnumerable<CacheDirective> cacheControl = null,
            ContentInfo contentInfo = null,
            bool? expectContinue = null,
            Guid? id = null,
            IEnumerable<Tuple<Header, object>> headers = null,
            Method meth = null, 
            IEnumerable<CacheDirective> pragma = null,
            RequestPreconditions preconditions = null,
            RequestPreferences preferences = null,
            ChallengeMessage proxyAuthorization = null,
            Uri referer = null,
            Uri uri = null,
            UserAgent userAgent = null,
            HttpVersion version = null)
        {
            return new HttpRequest<TReq> (
                authorization != null ? FSharpOption<ChallengeMessage>.Some(authorization) : This.Authorization,
                cacheControl != null ? SetModule.OfSeq<CacheDirective>(cacheControl) : This.CacheControl,
                contentInfo != null ? contentInfo : This.ContentInfo,
                This.Entity, 
                expectContinue != null ? expectContinue.Value : This.ExpectContinue,
                headers != null ? MapModule.OfSeq<Header, object>(headers) : This.Headers,
                id != null ? id.Value : This.Id,
                meth != null ? meth : This.Method,
                pragma != null ? SetModule.OfSeq <CacheDirective>(pragma) : This.Pragma,
                preconditions != null ? preconditions : This.Preconditions,
                preferences != null ? preferences : This.Preferences,
                proxyAuthorization != null ? FSharpOption<ChallengeMessage>.Some(proxyAuthorization) : This.ProxyAuthorization,
                referer != null ? FSharpOption<Uri>.Some(referer) : This.Referer,
                uri != null ? uri : This.Uri,
                userAgent != null ? FSharpOption<UserAgent>.Some(userAgent) : This.UserAgent,
                version != null ? version : This.Version);
        }

        public static HttpRequest<TNew> With<TReq, TNew>(
            this HttpRequest<TReq> This,
            TNew entity, 
            ChallengeMessage authorization = null,
            IEnumerable<CacheDirective> cacheControl = null,
            ContentInfo contentInfo = null,
            bool? expectContinue = null,
            IEnumerable<Tuple<Header, object>> headers = null,
            Guid? id = null,
            Method meth = null,
            IEnumerable<CacheDirective> pragma = null,
            RequestPreconditions preconditions = null,
            RequestPreferences preferences = null,
            ChallengeMessage proxyAuthorization = null,
            Uri referer = null,
            Uri uri = null,
            UserAgent userAgent = null,
            HttpVersion version = null)
        {
            Contract.Requires(entity != null);

            return new HttpRequest<TNew>(
                authorization != null ? FSharpOption<ChallengeMessage>.Some(authorization) : This.Authorization,
                cacheControl != null ? SetModule.OfSeq<CacheDirective>(cacheControl) : This.CacheControl,
                contentInfo != null ? contentInfo : This.ContentInfo,
                FSharpOption<TNew>.Some(entity),
                expectContinue != null ? expectContinue.Value : This.ExpectContinue,
                headers != null ? MapModule.OfSeq<Header, object>(headers) : This.Headers,
                id != null ? id.Value : This.Id,
                meth != null ? meth : This.Method,
                pragma != null ? SetModule.OfSeq<CacheDirective>(pragma) : This.Pragma,
                preconditions != null ? preconditions : This.Preconditions,
                preferences != null ? preferences : This.Preferences,
                proxyAuthorization != null ? FSharpOption<ChallengeMessage>.Some(proxyAuthorization) : This.ProxyAuthorization,
                referer != null ? FSharpOption<Uri>.Some(referer) : This.Referer,
                uri != null ? uri : This.Uri,
                userAgent != null ? FSharpOption<UserAgent>.Some(userAgent) : This.UserAgent,
                version != null ? version : This.Version);
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
                authorization ? FSharpOption<ChallengeMessage>.None : This.Authorization,
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
                proxyAuthorization ? FSharpOption<ChallengeMessage>.None : This.ProxyAuthorization,
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
                authorization ? FSharpOption<ChallengeMessage>.None : This.Authorization,
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
                proxyAuthorization ? FSharpOption<ChallengeMessage>.None : This.ProxyAuthorization,
                referer ? FSharpOption<Uri>.None : This.Referer,
                This.Uri,
                userAgent ? FSharpOption<UserAgent>.None : This.UserAgent,
                This.Version);
        }

        public static bool TryGetEntity<TReq>(this HttpRequest<TReq> This, out TReq entity)
            where TReq:class
        {
            if (OptionModule.IsSome(This.Entity))
            {
                entity = This.Entity.Value;
                return true;
            }

            entity = null;
            return false;
        }

        public static bool TryGetAuthorization<TReq>(this HttpRequest<TReq> This, out ChallengeMessage authorization)
        {
            if (OptionModule.IsSome(This.Authorization))
            {
                authorization = This.Authorization.Value;
                return true;
            }

            authorization = null;
            return false;
        }

        public static bool TryGetProxyAuthorization<TReq>(this HttpRequest<TReq> This, out ChallengeMessage proxyAuthorization)
        {
            if (OptionModule.IsSome(This.ProxyAuthorization))
            {
                proxyAuthorization = This.ProxyAuthorization.Value;
                return true;
            }

            proxyAuthorization = null;
            return false;
        }

        public static bool TryGetReferer<TReq>(this HttpRequest<TReq> This, out Uri referer)
        {
            if (OptionModule.IsSome(This.Referer))
            {
                referer = This.Referer.Value;
                return true;
            }

            referer = null;
            return false;
        }

        public static bool TryGetUserAgent<TReq>(this HttpRequest<TReq> This, out UserAgent userAgent)
        {
            if (OptionModule.IsSome(This.UserAgent))
            {
                userAgent = This.UserAgent.Value;
                return true;
            }

            userAgent = null;
            return false;
        }
    }
}

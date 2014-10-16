using Microsoft.FSharp.Core;
using Microsoft.FSharp.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace FunctionalHttp
{
    public static class HttpRequestExtension
    {
        public static HttpRequest<TReq> With<TReq>(
            this HttpRequest<TReq> This, 
            Method meth = null, 
            Uri uri = null, 
            ContentInfo contentInfo = null,
            ChallengeMessage authorizationCredentials = null,
            IEnumerable<CacheDirective> cacheDirectives = null,
            Uri referer = null)
        {
            return new HttpRequest<TReq> (
                meth != null ? meth : This.Method,
                uri != null ? uri : This.Uri,
                FSharpOption<TReq>.None,
                This.Id,
                contentInfo != null ? contentInfo : This.ContentInfo,
                authorizationCredentials != null ? FSharpOption<ChallengeMessage>.Some(authorizationCredentials) : This.AuthorizationCredentials,
                SetModule.OfSeq<CacheDirective>(cacheDirectives != null ? cacheDirectives : This.CacheDirectives),
                referer != null ? FSharpOption<Uri>.Some(referer) : This.Referer);
        }

        public static HttpRequest<TNew> With<TReq, TNew>(
            this HttpRequest<TReq> This,
            TNew entity, 
            Method meth = null,
            Uri uri = null,
            ContentInfo contentInfo = null,
            ChallengeMessage authorizationCredentials = null,
            IEnumerable<CacheDirective> cacheDirectives = null,
            Uri referer = null)
        {
            Contract.Requires(entity != null);

            return new HttpRequest<TNew>(
                meth != null ? meth : This.Method,
                uri != null ? uri : This.Uri,
                FSharpOption<TNew>.Some(entity),
                This.Id,
                contentInfo != null ? contentInfo : This.ContentInfo,
                authorizationCredentials != null ? FSharpOption<ChallengeMessage>.Some(authorizationCredentials) : This.AuthorizationCredentials,
                SetModule.OfSeq<CacheDirective>(cacheDirectives != null ? cacheDirectives : This.CacheDirectives),
                referer != null ? FSharpOption<Uri>.Some(referer) : This.Referer);
        }

        public static HttpRequest<TReq> Without<TReq>(this HttpRequest<TReq> This, bool contentInfo = false, bool authorizationCredentials = false, bool cacheDirectives = false, bool referer = false)
        {
            return new HttpRequest<TReq>(
                This.Method,
                This.Uri,
                This.Entity,
                This.Id,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                authorizationCredentials ? FSharpOption<ChallengeMessage>.None : This.AuthorizationCredentials,
                SetModule.OfSeq<CacheDirective>(cacheDirectives ? SeqModule.Empty<CacheDirective>() : This.CacheDirectives),
                referer ? FSharpOption<Uri>.None : This.Referer);
        }

        public static HttpRequest<TNew> WithoutEntity<TReq, TNew>(this HttpRequest<TReq> This, bool contentInfo = false, bool authorizationCredentials = false, bool cacheDirectives = false, bool referer = false)
        {
            return new HttpRequest<TNew>(
                This.Method,
                This.Uri,
                FSharpOption<TNew>.None,
                This.Id,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                authorizationCredentials ? FSharpOption<ChallengeMessage>.None : This.AuthorizationCredentials,
                SetModule.OfSeq<CacheDirective>(cacheDirectives ? SeqModule.Empty<CacheDirective>() : This.CacheDirectives),
                referer ? FSharpOption<Uri>.None : This.Referer);
        }
    }
}

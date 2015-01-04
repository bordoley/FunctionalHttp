using FunctionalHttp.Collections;
using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using System;
using System.Collections.Generic;

using FunctionalHttp;

namespace FunctionalHttp.Core.Interop
{
    public static class HttpResponseExtensionsCSharp
    {
        public static HttpResponse<TResp> With<TResp>(
            this HttpResponse<TResp> This,
            AcceptableRanges acceptedRanges = null,
            TimeSpan? age = null,
            IEnumerable<Method> allowed = null,
            IEnumerable<Challenge> authenticate = null,
            IEnumerable<CacheDirective> cacheControl = null,
            ContentInfo contentInfo = null,
            DateTime? date = null,
            EntityTag etag = null,
            DateTime? expires = null,
            IEnumerable<Tuple<Header, object>> headers = null,
            Guid? id = null,
            DateTime? lastModified = null,
            Uri location = null,
            IEnumerable<Challenge> proxyAuthenticate = null,
            DateTime? retryAfter = null,
            Server server = null,
            Status status = null,
            Vary vary = null,
            HttpVersion? version = null,
            IEnumerable<Warning> warning = null)
        {
            return HttpResponseInternal.With<TResp, TResp>(
                acceptedRanges.ToFSharpOption(),
                age.ToFSharpOption(),
                allowed.ToFSharpOption(),
                authenticate.ToFSharpOption(),
                cacheControl.ToFSharpOption(), 
                contentInfo.ToFSharpOption(), 
                date.ToFSharpOption(),
                This.Entity,
                etag.ToFSharpOption(),
                expires.ToFSharpOption(), 
                headers.ToFSharpOption().Select(MapModule.OfSeq),
                id.ToFSharpOption(),
                lastModified.ToFSharpOption(),
                location.ToFSharpOption(), 
                proxyAuthenticate.ToFSharpOption(),
                retryAfter.ToFSharpOption(),
                server.ToFSharpOption(),
                status.ToFSharpOption(),
                vary.ToFSharpOption(),
                version.ToFSharpOption(),
                warning.ToFSharpOption(),
                This);
        }

        public static HttpResponse<TNew> With<TResp, TNew>(
            this HttpResponse<TResp> This,
            TNew entity,
            AcceptableRanges acceptedRanges = null,
            TimeSpan? age = null,
            IEnumerable<Method> allowed = null,
            IEnumerable<Challenge> authenticate = null,
            IEnumerable<CacheDirective> cacheControl = null,
            ContentInfo contentInfo = null,
            DateTime? date = null,
            EntityTag etag = null,
            DateTime? expires = null,
            IEnumerable<Tuple<Header, object>> headers = null,
            Guid? id = null,
            DateTime? lastModified = null, 
            Uri location = null,
            IEnumerable<Challenge> proxyAuthenticate = null,
            DateTime? retryAfter = null,
            Server server = null,
            Status status = null,
            Vary vary = null,
            HttpVersion? version = null,
            IEnumerable<Warning> warning = null)
        {
            return HttpResponseInternal.With<TResp, TNew>(
                acceptedRanges.ToFSharpOption(),
                age.ToFSharpOption(), 
                allowed.ToFSharpOption(),
                authenticate.ToFSharpOption(),
                cacheControl.ToFSharpOption(),
                contentInfo.ToFSharpOption(),
                date.ToFSharpOption(),
                entity,
                etag.ToFSharpOption(),
                expires.ToFSharpOption(),
                headers.ToFSharpOption().Select(MapModule.OfSeq),
                id.ToFSharpOption(),
                lastModified.ToFSharpOption(),
                location.ToFSharpOption(),
                proxyAuthenticate.ToFSharpOption(),
                retryAfter.ToFSharpOption(),
                server.ToFSharpOption(),
                status.ToFSharpOption(),
                vary.ToFSharpOption(),
                version.ToFSharpOption(),
                warning.ToFSharpOption(),
                This);
        }

        public static HttpResponse<TResp> Without<TResp>(
            this HttpResponse<TResp> This,
            bool acceptedRanges = false,
            bool age = false,
            bool allowed = false,
            bool authenticate = false,
            bool cacheControl = false,
            bool contentInfo = false,
            bool date = false,
            bool etag = false,
            bool expires = false,
            bool headers = false,
            bool lastModified = false,
            bool location = false,
            bool proxyAuthenticate = false,
            bool retryAfter = false,
            bool server = false,
            bool vary = false,
            bool warning = false)
        {
            return HttpResponseInternal.Without<TResp>(
                acceptedRanges,
                age, 
                allowed,
                authenticate, 
                cacheControl, 
                contentInfo, 
                date, 
                etag, 
                expires, 
                headers, 
                lastModified, 
                location, 
                proxyAuthenticate,
                retryAfter,
                server,
                vary,
                warning,
                This);
        }
    }
}

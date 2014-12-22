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
            HttpVersion version = null,
            IEnumerable<Warning> warning = null)
        {
            return new HttpResponse<TResp>(
                acceptedRanges != null ? FSharpOption<AcceptableRanges>.Some(acceptedRanges) : This.AcceptedRanges,
                age != null ? FSharpOption<TimeSpan>.Some(age.Value) : This.Age,
                allowed != null ? SetModule.OfSeq<Method>(allowed) : This.Allowed,
                authenticate != null ? SetModule.OfSeq<Challenge>(authenticate) : This.Authenticate,
                cacheControl != null ? SetModule.OfSeq<CacheDirective>(cacheControl) : This.CacheControl,
                contentInfo != null ? contentInfo : This.ContentInfo,
                date != null ? FSharpOption<DateTime>.Some(date.Value) : This.Date,
                This.Entity,
                etag != null ? FSharpOption<EntityTag>.Some(etag) : This.ETag,
                expires != null ? FSharpOption<DateTime>.Some(expires.Value) : This.Expires,
                headers != null ? MapModule.OfSeq<Header, object>(headers) : This.Headers,
                id != null ? id.Value : This.Id,
                lastModified != null ? FSharpOption<DateTime>.Some(lastModified.Value) : This.LastModified,
                location != null ? FSharpOption<Uri>.Some(location) : This.Location,
                proxyAuthenticate != null ? SetModule.OfSeq<Challenge>(proxyAuthenticate) : This.ProxyAuthenticate,
                retryAfter != null ? FSharpOption<DateTime>.Some(retryAfter.Value) : This.RetryAfter,
                server != null ? FSharpOption<Server>.Some(server) : This.Server,
                status != null ? status : This.Status,
                vary != null ? FSharpOption<Vary>.Some(vary) : This.Vary,
                version != null ? version : This.Version,
                warning != null ? ListModule.OfSeq(warning) : This.Warning);
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
            HttpVersion version = null,
            IEnumerable<Warning> warning = null)
        {
            return new HttpResponse<TNew>(
                acceptedRanges != null ? FSharpOption<AcceptableRanges>.Some(acceptedRanges) : This.AcceptedRanges,
                age != null ? FSharpOption<TimeSpan>.Some(age.Value) : This.Age,
                allowed != null ? SetModule.OfSeq<Method>(allowed) : This.Allowed,
                authenticate != null ? SetModule.OfSeq<Challenge>(authenticate) : This.Authenticate,
                cacheControl != null ? SetModule.OfSeq<CacheDirective>(cacheControl) : This.CacheControl,
                contentInfo != null ? contentInfo : This.ContentInfo,
                date != null ? FSharpOption<DateTime>.Some(date.Value) : This.Date,
                FSharpOption<TNew>.Some(entity),
                etag != null ? FSharpOption<EntityTag>.Some(etag) : This.ETag,
                expires != null ? FSharpOption<DateTime>.Some(expires.Value) : This.Expires,
                headers != null ? MapModule.OfSeq<Header, object>(headers) : This.Headers,
                id != null ? id.Value : This.Id,
                lastModified != null ? FSharpOption<DateTime>.Some(lastModified.Value) : This.LastModified,
                location != null ? FSharpOption<Uri>.Some(location) : This.Location,
                proxyAuthenticate != null ? SetModule.OfSeq<Challenge>(proxyAuthenticate) : This.ProxyAuthenticate,
                retryAfter != null ? FSharpOption<DateTime>.Some(retryAfter.Value) : This.RetryAfter,
                server != null ? FSharpOption<Server>.Some(server) : This.Server,
                status != null ? status : This.Status,
                vary != null ? FSharpOption<Vary>.Some(vary) : This.Vary,
                version != null ? version : This.Version,
                warning != null ? ListModule.OfSeq(warning) : This.Warning);
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
            return new HttpResponse<TResp>(
                acceptedRanges ?  FSharpOption<AcceptableRanges>.None : This.AcceptedRanges,
                age ? FSharpOption<TimeSpan>.None : This.Age,
                allowed ? SetModule.Empty<Method>() : This.Allowed,
                authenticate ? SetModule.Empty<Challenge>() : This.Authenticate,
                cacheControl ? SetModule.Empty<CacheDirective>() : This.CacheControl, 
                contentInfo ? ContentInfo.None : This.ContentInfo,
                date ? FSharpOption<DateTime>.None : This.Date,
                This.Entity,
                etag ? FSharpOption<EntityTag>.None : This.ETag,
                expires ? FSharpOption<DateTime>.None : This.Expires,
                headers ? MapModule.Empty<Header, object>() : This.Headers,
                This.Id,
                lastModified ? FSharpOption<DateTime>.None : This.LastModified,
                location ? FSharpOption<Uri>.None : This.Location,
                proxyAuthenticate ? SetModule.Empty<Challenge>() : This.ProxyAuthenticate,
                retryAfter ? FSharpOption<DateTime>.None : This.RetryAfter,
                server ? FSharpOption<Server>.None : This.Server,
                This.Status,
                vary ? FSharpOption<Vary>.None : This.Vary,
                This.Version,
                warning ? ListModule.Empty<Warning>() : This.Warning);
        }

        public static HttpResponse<TNew> WithoutEntity<TResp, TNew>(
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
            return new HttpResponse<TNew>(
                acceptedRanges ? FSharpOption<AcceptableRanges>.None : This.AcceptedRanges,
                age ? FSharpOption<TimeSpan>.None : This.Age,
                allowed ? SetModule.Empty<Method>() : This.Allowed,
                authenticate ? SetModule.Empty<Challenge>() : This.Authenticate,
                cacheControl ? SetModule.Empty<CacheDirective>() : This.CacheControl,
                contentInfo ? ContentInfo.None : This.ContentInfo,
                date ? FSharpOption<DateTime>.None : This.Date,
                FSharpOption<TNew>.None,
                etag ? FSharpOption<EntityTag>.None : This.ETag,
                expires ? FSharpOption<DateTime>.None : This.Expires,
                headers ? MapModule.Empty<Header, object>() : This.Headers,
                This.Id,
                lastModified ? FSharpOption<DateTime>.None : This.LastModified,
                location ? FSharpOption<Uri>.None : This.Location,
                proxyAuthenticate ? SetModule.Empty<Challenge>() : This.ProxyAuthenticate,
                retryAfter ? FSharpOption<DateTime>.None : This.RetryAfter,
                server ? FSharpOption<Server>.None : This.Server,
                This.Status,
                vary ? FSharpOption<Vary>.None : This.Vary,
                This.Version,
                warning ? ListModule.Empty<Warning>() : This.Warning);
        }
    }
}

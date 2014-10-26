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
            TimeSpan? age = null,
            IEnumerable<Method> allowed = null,
            IEnumerable<ChallengeMessage> authenticate = null,
            IEnumerable<CacheDirective> cacheControl = null,
            ContentInfo contentInfo = null,
            DateTime? date = null,
            EntityTag etag = null,
            DateTime? expires = null,
            IEnumerable<Tuple<Header, object>> headers = null,
            Guid? id = null,
            DateTime? lastModified = null,
            Uri location = null,
            DateTime? retryAfter = null,
            Server server = null,
            Status status = null,
            HttpVersion version = null)
        {
            return new HttpResponse<TResp>(
                age != null ? FSharpOption<TimeSpan>.Some(age.Value) : This.Age,
                allowed != null ? SetModule.OfSeq<Method>(allowed) : This.Allowed,
                authenticate != null ? SetModule.OfSeq<ChallengeMessage>(authenticate) : This.Authenticate,
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
                retryAfter != null ? FSharpOption<DateTime>.Some(retryAfter.Value) : This.RetryAfter,
                server != null ? FSharpOption<Server>.Some(server) : This.Server,
                status != null ? status : This.Status,
                version != null ? version : This.Version);
        }

        public static HttpResponse<TNew> With<TResp, TNew>(
            this HttpResponse<TResp> This,
            TNew entity,
            TimeSpan? age = null,
            IEnumerable<Method> allowed = null,
            IEnumerable<ChallengeMessage> authenticate = null,
            IEnumerable<CacheDirective> cacheControl = null,
            ContentInfo contentInfo = null,
            DateTime? date = null,
            EntityTag etag = null,
            DateTime? expires = null,
            IEnumerable<Tuple<Header, object>> headers = null,
            Guid? id = null,
            DateTime? lastModified = null, 
            Uri location = null,
            DateTime? retryAfter = null,
            Server server = null,
            Status status = null,
            HttpVersion version = null)
        {
            return new HttpResponse<TNew>(
               age != null ? FSharpOption<TimeSpan>.Some(age.Value) : This.Age,
               allowed != null ? SetModule.OfSeq<Method>(allowed) : This.Allowed,
               authenticate != null ? SetModule.OfSeq<ChallengeMessage>(authenticate) : This.Authenticate,
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
               retryAfter != null ? FSharpOption<DateTime>.Some(retryAfter.Value) : This.RetryAfter,
               server != null ? FSharpOption<Server>.Some(server) : This.Server,
               status != null ? status : This.Status,
               version != null ? version : This.Version);
        }

        public static HttpResponse<TResp> Without<TResp>(
            this HttpResponse<TResp> This,
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
            bool retryAfter = false,
            bool server = false)
        {
            return new HttpResponse<TResp>(
                age ? FSharpOption<TimeSpan>.None : This.Age,
                allowed ? SetModule.Empty<Method>() : This.Allowed,
                authenticate ? SetModule.Empty<ChallengeMessage>() : This.Authenticate,
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
                retryAfter ? FSharpOption<DateTime>.None : This.RetryAfter,
                server ? FSharpOption<Server>.None : This.Server,
                This.Status,
                This.Version);
        }

        public static HttpResponse<TNew> WithoutEntity<TResp, TNew>(
            this HttpResponse<TResp> This,
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
            bool retryAfter = false,
            bool server = false)
        {
            return new HttpResponse<TNew>(
                age ? FSharpOption<TimeSpan>.None : This.Age,
                allowed ? SetModule.Empty<Method>() : This.Allowed,
                authenticate ? SetModule.Empty<ChallengeMessage>() : This.Authenticate,
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
                retryAfter ? FSharpOption<DateTime>.None : This.RetryAfter,
                server ? FSharpOption<Server>.None : This.Server,
                This.Status,
                This.Version);
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

        public static bool TryGetDate<TResp>(this HttpResponse<TResp> This, out DateTime date)
        {
            if (OptionModule.IsSome(This.Date))
            {
                date = This.Date.Value;
                return true;
            }

            date = DateTime.MinValue;
            return false;
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

        public static bool TryGetETag<TResp>(this HttpResponse<TResp> This, out EntityTag etag)
        {
            if (OptionModule.IsSome(This.ETag))
            {
                etag = This.ETag.Value;
                return true;
            }

            etag = null;
            return false;
        }

        public static bool TryGetExpires<TResp>(this HttpResponse<TResp> This, out DateTime expires)
        {
            if (OptionModule.IsSome(This.Expires))
            {
                expires = This.Expires.Value;
                return true;
            }

            expires = DateTime.MinValue;
            return false;
        }

        public static bool TryGetLastModified<TResp>(this HttpResponse<TResp> This, out DateTime lastModified)
        {
            if (OptionModule.IsSome(This.Expires))
            {
                lastModified = This.LastModified.Value;
                return true;
            }

            lastModified = DateTime.MinValue;
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

        public static bool TryGetRetryAfter<TResp>(this HttpResponse<TResp> This, out DateTime retryAfter)
        {
            if (OptionModule.IsSome(This.RetryAfter))
            {
                retryAfter = This.RetryAfter.Value;
                return true;
            }

            retryAfter = DateTime.MinValue;
            return false;
        }

        public static bool TryGetServer<TResp>(this HttpResponse<TResp> This, out Server server)
        {
            if (OptionModule.IsSome(This.Server))
            {
                server = This.Server.Value;
                return true;
            }

            server = null;
            return false;
        }
    }
}

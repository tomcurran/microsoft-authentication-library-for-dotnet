﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Identity.Client.Internal.Requests;

namespace Microsoft.Identity.Client.OAuth2.Throttling
{
    internal class UiRequiredProvider : IThrottlingProvider
    {
       
        /// <summary>
        /// Default number of seconds that application returns the cached response, in case of UI required requests.
        /// </summary>
        internal static readonly TimeSpan s_uiRequiredExpiration = TimeSpan.FromSeconds(120);

        /// <summary>
        /// Exposed only for testing purposes
        /// </summary>
        internal ThrottlingCache Cache { get; }

        public UiRequiredProvider()
        {
            Cache = new ThrottlingCache();
        }

        private static readonly ISet<string> s_excludedParams = new HashSet<string>()
        {
            
        };

        public void RecordException(AuthenticationRequestParameters requestParams, IReadOnlyDictionary<string, string> bodyParams, MsalServiceException ex)
        {
            if (ex is MsalUiRequiredException)
            {
                var logger = requestParams.RequestContext.Logger;

                logger.Info($"[Throttling] MsalUiRequiredException encountered - " +
                    $"throttling for {s_uiRequiredExpiration.TotalSeconds} seconds");

                var thumbprint = GetRequestFullThumbprint(bodyParams,
                    requestParams.AuthorityInfo.CanonicalAuthority,
                    requestParams.Account?.HomeAccountId?.Identifier);
                var entry = new ThrottlingCacheEntry(ex, s_uiRequiredExpiration);
                Cache.AddAndCleanup(thumbprint, entry, logger);
            }
        }

        public void ResetCache()
        {
            Cache.Clear();
        }

        public void TryThrottle(AuthenticationRequestParameters requestParams, IReadOnlyDictionary<string, string> bodyParams)
        {
            if (!Cache.IsEmpty())
            {
                var logger = requestParams.RequestContext.Logger;

                string fullThumbprint = GetRequestFullThumbprint(
                    bodyParams,
                    requestParams.AuthorityInfo.CanonicalAuthority,
                    requestParams.Account?.HomeAccountId?.Identifier);

                ThrottleCommon.TryThrow(fullThumbprint, Cache, logger, nameof(UiRequiredProvider));
            }
        }

        private static string GetRequestFullThumbprint(
             IReadOnlyDictionary<string, string> bodyParams,
             string authority,
             string homeAccountId)
        {
            string strictThumbprint = ThrottleCommon.GetRequestStrictThumbprint(bodyParams, authority, homeAccountId);
            string additionalParams = string.Concat(
                bodyParams.Values.Except(s_excludedParams).OrderBy(val => val, StringComparer.OrdinalIgnoreCase));

            return $"{strictThumbprint}.{additionalParams}";
        }

    }
}

// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Identity.Client.PlatformsCommon.Interfaces;
using Microsoft.Identity.Client.Utils;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Identity.Client.Internal
{
#if !ANDROID_BUILDTIME && !iOS_BUILDTIME && !WINDOWS_APP_BUILDTIME && !MAC_BUILDTIME // Hide confidential client on mobile platforms

    /// <summary>
    /// Meant to be used in confidential client applications, an instance of <c>ClientCredential</c> is passed
    /// to the constructors of (<see cref="ConfidentialClientApplication"/>)
    /// as credentials proving that the application (the client) is what it claims it is. These credentials can be
    /// either a client secret (an application password) or a certificate.
    /// This class has one constructor for each case.
    /// These credentials are added in the application registration portal (in the secret section).
    /// </summary>
    internal sealed class ClientCredentialWrapper
    {
        public ClientCredentialWrapper(ApplicationConfiguration config)
        {
            ConfidentialClientApplication.GuardMobileFrameworks();

            ValidateCredentialParameters(config);

            switch (AuthenticationType)
            {
                case ConfidentialClientAuthenticationType.ClientCertificate:
                    SigningCredentials = new X509SigningCredentials(config.ClientCredentialCertificate);
                    //Certificate = config.ClientCredentialCertificate;
                    break;
                case ConfidentialClientAuthenticationType.ClientCertificateWithClaims:
                    SigningCredentials = new X509SigningCredentials(config.ClientCredentialCertificate);
                    //Certificate = config.ClientCredentialCertificate;
                    ClaimsToSign = config.ClaimsToSign;
                    break;
                case ConfidentialClientAuthenticationType.ClientSecret:
                    Secret = config.ClientSecret;
                    break;
                case ConfidentialClientAuthenticationType.SignedClientAssertion:
                    SignedAssertion = config.SignedClientAssertion;
                    break;
                case ConfidentialClientAuthenticationType.SigningCredentials:
                    SigningCredentials = config.SigningCredentials;
                    break;
                default:
                    throw new NotImplementedException(AuthenticationType.ToString());
            }
        }

        //#region TestBuilders
        ////The following builders methods are inteded for testing
        //public static ClientCredentialWrapper CreateWithCertificate(X509Certificate2 certificate, IDictionary<string, string> claimsToSign = null)
        //{
        //    return new ClientCredentialWrapper(certificate, claimsToSign);
        //}

        //public static ClientCredentialWrapper CreateWithSecret(string secret)
        //{
        //    var app = new ClientCredentialWrapper(secret, ConfidentialClientAuthenticationType.ClientSecret);
        //    app.AuthenticationType = ConfidentialClientAuthenticationType.ClientSecret;
        //    return app;
        //}

        //public static ClientCredentialWrapper CreateWithSignedClientAssertion(string signedClientAssertion)
        //{
        //    var app = new ClientCredentialWrapper(signedClientAssertion, ConfidentialClientAuthenticationType.SignedClientAssertion);
        //    app.AuthenticationType = ConfidentialClientAuthenticationType.SignedClientAssertion;
        //    return app;
        //}

        //private ClientCredentialWrapper(X509Certificate2 certificate, IDictionary<string, string> claimsToSign = null)
        //{
        //    ConfidentialClientApplication.GuardMobileFrameworks();

        //    Certificate = certificate;

        //    if (claimsToSign != null && claimsToSign.Any())
        //    {
        //        ClaimsToSign = claimsToSign;
        //        AuthenticationType = ConfidentialClientAuthenticationType.ClientCertificateWithClaims;
        //        return;
        //    }

        //    AuthenticationType = ConfidentialClientAuthenticationType.ClientCertificate;
        //}

        //private ClientCredentialWrapper(string secretOrAssertion, ConfidentialClientAuthenticationType authType)
        //{
        //    ConfidentialClientApplication.GuardMobileFrameworks();

        //    if (authType == ConfidentialClientAuthenticationType.SignedClientAssertion)
        //    {
        //        SignedAssertion = secretOrAssertion;
        //    }
        //    else
        //    {
        //        Secret = secretOrAssertion;
        //    }
        //}

        //#endregion TestBuilders


        private void ValidateCredentialParameters(ApplicationConfiguration config)
        {
            if (config.ConfidentialClientCredentialCount > 1)
            {
                throw new MsalClientException(MsalError.ClientCredentialAuthenticationTypesAreMutuallyExclusive, MsalErrorMessage.ClientCredentialAuthenticationTypesAreMutuallyExclusive);
            }

            if (!string.IsNullOrWhiteSpace(config.ClientSecret))
            {
                AuthenticationType = ConfidentialClientAuthenticationType.ClientSecret;
                return;
            }

            if (config.ClientCredentialCertificate != null)
            {
                if (config.ClaimsToSign != null && config.ClaimsToSign.Any())
                {
                    AuthenticationType = ConfidentialClientAuthenticationType.ClientCertificateWithClaims;
                    AppendDefaultClaims = config.MergeWithDefaultClaims;
                    return;
                }

                AuthenticationType = ConfidentialClientAuthenticationType.ClientCertificate;
                return;
            }

            if (!string.IsNullOrWhiteSpace(config.SignedClientAssertion))
            {
                AuthenticationType = ConfidentialClientAuthenticationType.SignedClientAssertion;
            }

            if (config.SigningCredentials != null)
            {
                AuthenticationType = ConfidentialClientAuthenticationType.SigningCredentials;
            }
        }

        internal byte[] Sign(ICryptographyManager cryptographyManager, string message)
        {
            // TODO: verify key size 

            byte[] sig = cryptographyManager.SignWithCertificate(
                message, ((X509SigningCredentials)SigningCredentials).Certificate);

            string s1 = Base64UrlHelpers.Encode(sig);

            var cryptoProviderFactory = SigningCredentials.CryptoProviderFactory ??
                SigningCredentials.Key.CryptoProviderFactory;
            var signatureProvider = cryptoProviderFactory.CreateForSigning(
                SigningCredentials.Key, SigningCredentials.Algorithm);
            if (signatureProvider == null)
                throw new MsalClientException("bad_signing_credentials", "Signing credentials cannot be used to sign");

            try
            {
                byte[] payload = Encoding.UTF8.GetBytes(message);
                byte[] signature = signatureProvider.Sign(payload);
                //string sig2 =  Base64UrlEncoder.Encode(signature);

                return signature;
            }
            finally
            {
                cryptoProviderFactory.ReleaseSignatureProvider(signatureProvider);
            }
        }

        public static int MinKeySizeInBits { get; } = 2048;
        internal string Thumbprint
        {
            get
            {
                // TODO: it looks like for X509, the Thumbprint is a hash of the cert 
                if (SigningCredentials is X509SigningCredentials)
                {

                    var hash = ((X509SigningCredentials)SigningCredentials).Certificate.GetCertHash();
                    string oriKid = Base64UrlEncoder.Encode(hash);
                    return oriKid;

                }

                return Base64UrlEncoder.Encode(SigningCredentials.Kid);
            }
        }

        //internal X509Certificate2 Certificate { get; private set; }
        internal SigningCredentials SigningCredentials { get; private set; }
        // The cached assertion created from the JWT signing operation
        internal string CachedAssertion { get; set; }
        internal long ValidTo { get; set; }
        internal bool ContainsX5C { get; set; }
        internal string Audience { get; set; }
        internal string Secret { get; private set; }
        // The signed assertion passed in by the user
        internal string SignedAssertion { get; private set; }
        internal bool AppendDefaultClaims { get; private set; }
        internal ConfidentialClientAuthenticationType AuthenticationType { get; private set; }
        internal IDictionary<string, string> ClaimsToSign { get; private set; }
    }

    internal enum ConfidentialClientAuthenticationType
    {
        ClientCertificate,
        ClientCertificateWithClaims,
        ClientSecret,
        SignedClientAssertion,
        SigningCredentials
    }
#endif
}

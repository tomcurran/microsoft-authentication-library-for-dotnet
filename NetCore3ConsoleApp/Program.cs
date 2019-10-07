// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace NetCore3ConsoleApp
{
    public class Program
    {
        // This app has http://localhost redirect uri registered
        private static readonly string s_clientIdForPublicApp = "1d18b3b0-251b-4714-a02a-9956cec86c2d";
        private static readonly IEnumerable<string> s_scopes = new[] { "user.read" }; // used for WIA and U/P, can be empty
        private const string GraphAPIEndpoint = "https://graph.microsoft.com/v1.0/me";

        public static readonly string CacheFilePath = System.Reflection.Assembly.GetExecutingAssembly().Location + ".msalcache.json";

        static void Main(string[] args)
        {
            var publicClient = CreatePublicClientApplication();
            RunConsoleAppLogicAsync(publicClient).Wait();
        }

        private static IPublicClientApplication CreatePublicClientApplication()
        {
            IPublicClientApplication pca = PublicClientApplicationBuilder
                .Create(s_clientIdForPublicApp)
                .WithLogging(Log, LogLevel.Verbose, true)
                .WithRedirectUri("http://localhost")
                .Build();

            pca.UserTokenCache.SetBeforeAccess(notficationArgs =>
            {
                notficationArgs.TokenCache.DeserializeMsalV3(File.Exists(CacheFilePath)
                    ? File.ReadAllBytes(CacheFilePath)
                    : null);
            });

            pca.UserTokenCache.SetAfterAccess(notificationArgs =>
            {
                // if the access operation resulted in a cache update
                if (notificationArgs.HasStateChanged)
                {
                    // reflect changes in the persistent stores
                    File.WriteAllBytes(CacheFilePath, notificationArgs.TokenCache.SerializeMsalV3());
                }
            });

            return pca;
        }

        private static async Task RunConsoleAppLogicAsync(IPublicClientApplication pca)
        {
            while(true)
            {
                Console.Clear();

                // display menu
                Console.WriteLine(@"
                        1. Acquire Token Interactive
                        0. Exit App
                    Enter your selection: ");

                int.TryParse(Console.ReadLine(), out var selection);

                Task<AuthenticationResult> authTask = null;

                try
                {
                    switch (selection)
                    {
                        case 1: // acquire token interactive
                            authTask = pca.AcquireTokenInteractive(s_scopes)
                                .ExecuteAsync(CancellationToken.None);

                            // call graph here
                            break;

                        case 0:
                            return;
                        default:
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Log(LogLevel.Error, ex.Message, false);
                    Log(LogLevel.Error, ex.StackTrace, false);
                }

                Console.WriteLine("\n\nHit 'ENTER' to continue...");
                Console.ReadLine();
            }
        }

        private static void Log(LogLevel level, string message, bool containsPii)
        {
            if (!containsPii)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
            }

            switch (level)
            {
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                case LogLevel.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Verbose:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                default:
                    break;
            }

            Console.WriteLine($"{level} {message}");
            Console.ResetColor();
        }
    }
}

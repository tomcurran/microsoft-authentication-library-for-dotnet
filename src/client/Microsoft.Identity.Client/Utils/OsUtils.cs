// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Identity.Client.Utils
{
    internal class OsUtils
    {
        /// <summary>
        ///  Is this a windows platform
        /// </summary>
        /// <returns>A  value indicating if we are running on windows or not</returns>
        public static bool IsWindowsPlatform()
        {
#if !NETSTANDARD1_3
            return Environment.OSVersion.Platform == PlatformID.Win32NT;
#else
            return false;
#endif
        }

        /// <summary>
        /// Is this a MAC platform
        /// </summary>
        /// <returns>A value indicating if we are running on mac or not</returns>
        public static bool IsMacPlatform()
        {
#if DESKTOP
            // we have to also check for PlatformID.Unix because Mono can sometimes return Unix as the platform on a Mac machine.
            // see http://www.mono-project.com/docs/faq/technical/
            return Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
#else
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
#endif
        }

        /// <summary>
        /// Is this a linux platform
        /// </summary>
        /// <returns>A  value indicating if we are running on linux or not</returns>
        public static bool IsLinuxPlatform()
        {
#if DESKTOP
            return Environment.OSVersion.Platform == PlatformID.Unix;
#else
            return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
#endif
        }

    }
}

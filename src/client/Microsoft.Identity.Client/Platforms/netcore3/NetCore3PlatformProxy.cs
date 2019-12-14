using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Platforms.Shared.NetCore;
using Microsoft.Identity.Client.Platforms.Shared.NetStdCore;
using Microsoft.Identity.Client.PlatformsCommon.Shared;
using Microsoft.Identity.Client.UI;
using Microsoft.Identity.Client.Utils;

namespace Microsoft.Identity.Client.Platforms.netcore3
{
    /// <summary>
    /// Platform / OS specific logic.  No library (ADAL / MSAL) specific code should go in here.
    /// </summary>
    internal class NetCore3PlatformProxy : NetCorePlatformProxyShared
    {
        public NetCore3PlatformProxy(ICoreLogger logger)
            : base(logger)
        {
        }

        protected override IWebUIFactory CreateWebUiFactory()
        {
            return new NetCore3WebUIFactory();
        }

        public override bool UseEmbeddedWebViewDefault
        {
            get
            {
                // TODO add commment about mac/linux and windows diff
                return OsUtils.IsWindowsPlatform();
            }
        }
    }
}

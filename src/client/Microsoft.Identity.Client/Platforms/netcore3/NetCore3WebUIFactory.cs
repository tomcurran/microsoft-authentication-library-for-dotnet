// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Platforms.Shared.Desktop.OsBrowser;
using Microsoft.Identity.Client.Platforms.Shared.WinForms;
using Microsoft.Identity.Client.UI;

namespace Microsoft.Identity.Client.Platforms.netcore3
{
    internal class NetCore3WebUIFactory : IWebUIFactory
    {
        public IWebUI CreateAuthenticationDialog(CoreUIParent coreUIParent, RequestContext requestContext)
        {
            // TODO: on windows return the default 
            // on mac and linux only use system browser - they don't have win forms
            if(coreUIParent.UseEmbeddedWebview && Utils.OsUtils.IsWindowsPlatform())
            {
                return new WinFormsWebUI(coreUIParent, requestContext);
            }

            return new DefaultOsBrowserWebUi(
               requestContext.ServiceBundle.PlatformProxy,
               requestContext.Logger,
               coreUIParent.SystemWebViewOptions);

        }
    }
}

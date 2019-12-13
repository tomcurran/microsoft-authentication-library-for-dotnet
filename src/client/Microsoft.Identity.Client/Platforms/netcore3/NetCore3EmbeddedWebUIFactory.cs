// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.Platforms.net45;
using Microsoft.Identity.Client.UI;

namespace Microsoft.Identity.Client.Platforms.netcore3
{
    internal class NetCore3EmbeddedWebUIFactory : IWebUIFactory
    {
        public IWebUI CreateAuthenticationDialog(CoreUIParent coreUIParent, RequestContext requestContext)
        {
            // TODO: on windows return the default 
            // on mac and linux only use system browser - they don't have win forms
            return new InteractiveWebUI(coreUIParent, requestContext);
        }
    }
}

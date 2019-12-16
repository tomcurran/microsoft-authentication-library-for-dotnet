// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.Identity.Client.Core;
using Microsoft.Identity.Client.UI;

namespace Microsoft.Identity.Client.Platforms.Shared.WinForms
{
    internal class WinFormsWebUI : WebUI
    {
        public WinFormsWebUI(CoreUIParent parent, RequestContext requestContext)
        {
            OwnerWindow = parent?.OwnerWindow;
            SynchronizationContext = parent?.SynchronizationContext;
            RequestContext = requestContext;
        }

        protected override AuthorizationResult OnAuthenticate()
        {
            AuthorizationResult result;

            using (WindowsFormsWebAuthenticationDialog _dialog = new WindowsFormsWebAuthenticationDialog(OwnerWindow) {RequestContext = RequestContext})
            {
                result = _dialog.AuthenticateAAD(RequestUri, CallbackUri);
            }

            return result;
        }
    }
}

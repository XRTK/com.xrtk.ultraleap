// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Attributes;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.InputSystem;
using XRTK.Ultraleap.Profiles;
using XRTK.Providers.Controllers;

namespace XRTK.Ultraleap.Providers.Controllers
{
    [RuntimePlatform(typeof(UltraleapPlatform))]
    [System.Runtime.InteropServices.Guid("bde6aef1-8b3e-4ca8-ac79-9c0279c04b4a")]
    public class UltraleapControllerDataProvider : BaseControllerDataProvider
    {
        /// <inheritdoc />
        public UltraleapControllerDataProvider(string name, uint priority, UltraleapControllerDataProviderProfile profile, IMixedRealityInputSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }
    }
}


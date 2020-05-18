// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Definitions.Controllers;
using XRTK.Definitions.Utilities;
using XRTK.Ultraleap.Providers.Controllers;

namespace XRTK.Ultraleap.Profiles
{
    public class UltraleapControllerDataProviderProfile : BaseMixedRealityControllerDataProviderProfile
    {
        public override ControllerDefinition[] GetDefaultControllerOptions()
        {
            return new[]
            {
                new ControllerDefinition(typeof(UltraleapController), Handedness.Left),
                new ControllerDefinition(typeof(UltraleapController), Handedness.Right)
            };
        }
    }
}


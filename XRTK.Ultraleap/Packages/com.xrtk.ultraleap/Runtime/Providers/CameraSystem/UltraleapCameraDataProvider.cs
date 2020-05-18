// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using XRTK.Attributes;
using XRTK.Definitions.CameraSystem;
using XRTK.Definitions.Platforms;
using XRTK.Interfaces.CameraSystem;
using XRTK.Providers.CameraSystem;

namespace XRTK.Ultraleap.Providers.CameraSystem
{
    [RuntimePlatform(typeof(UltraleapPlatform))]
    [System.Runtime.InteropServices.Guid("dad00b6c-bc99-45e4-b6ad-d450a4f7a0ac")]
    public class UltraleapCameraDataProvider : BaseCameraDataProvider
    {
        /// <inheritdoc />
        public UltraleapCameraDataProvider(string name, uint priority, BaseMixedRealityCameraDataProviderProfile profile, IMixedRealityCameraSystem parentService)
            : base(name, priority, profile, parentService)
        {
        }
    }
}

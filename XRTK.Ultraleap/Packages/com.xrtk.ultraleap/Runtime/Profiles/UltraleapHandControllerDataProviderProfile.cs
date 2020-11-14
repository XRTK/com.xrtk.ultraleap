// Copyright (c) XRTK. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using XRTK.Definitions.Controllers.Hands;
using XRTK.Definitions.Utilities;
using XRTK.Ultraleap.Definitions;

namespace XRTK.Ultraleap.Profiles
{
    /// <summary>
    /// Configuration profile for the <see cref="Providers.Controllers.UltraleapHandControllerDataProvider"/> powering
    /// the <see cref="XRTK.Providers.Controllers.Hands.MixedRealityHandController"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Mixed Reality Toolkit/Input System/Controller Data Providers/Ultraleap Hand", fileName = "UltraleapHandControllerDataProviderProfile", order = (int)CreateProfileMenuItemIndices.Input)]
    public class UltraleapHandControllerDataProviderProfile : BaseHandControllerDataProviderProfile
    {
        [SerializeField]
        [Tooltip("Specifies how the Ultraleap device is operated.")]
        private UltraleapOperationMode operationMode = UltraleapOperationMode.Desktop;

        /// <summary>
        /// Specifies how the Ultraleap device is operated.
        /// </summary>
        public UltraleapOperationMode OperationMode => operationMode;

        [SerializeField]
        [Tooltip("Applies only to UltraleapOperationMode.Desktop. Adds an offset to the recognized hand root position relative to the main camera, so hands are visible.")]
        private Vector3 leapControllerDesktopModeOffset = new Vector3(0, -0.2f, 0.2f);

        /// <summary>
        /// Applies only to UltraleapOperationMode.Desktop. Adds an offset to the recognized hand root position relative to the main camera, so hands are visible.
        /// </summary>
        public Vector3 LeapControllerDesktopModeOffset => leapControllerDesktopModeOffset;
    }
}
